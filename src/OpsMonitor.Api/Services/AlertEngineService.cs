using OpsMonitor.Api.Domain;
using SqlSugar;

namespace OpsMonitor.Api.Services;

public interface IAlertEngineService
{
    Task ProcessAsync(MonMonitor monitor, MonPolicy policy, MonCheckResult latestResult, CancellationToken ct = default);
}

public class AlertEngineService : IAlertEngineService
{
    private readonly ISqlSugarClient _db;
    private readonly INotificationService _notificationService;

    public AlertEngineService(ISqlSugarClient db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task ProcessAsync(MonMonitor monitor, MonPolicy policy, MonCheckResult latestResult, CancellationToken ct = default)
    {
        if (monitor.Type == MonitorType.Link)
        {
            await ProcessLinkAsync(monitor, policy, latestResult, ct);
            return;
        }

        if (monitor.Type == MonitorType.Cert)
        {
            await ProcessCertAsync(monitor, policy, latestResult, ct);
        }
    }

    private async Task ProcessLinkAsync(MonMonitor monitor, MonPolicy policy, MonCheckResult latestResult, CancellationToken ct)
    {
        var dedupKey = BuildDedupKey(monitor.Id, RuleType.LinkDown);
        var firing = await GetFiringAsync(dedupKey);

        var latest = await _db.Queryable<MonCheckResult>()
            .Where(x => x.MonitorId == monitor.Id)
            .OrderByDescending(x => x.Id)
            .Take(Math.Max(1, policy.FailThreshold))
            .ToListAsync();

        var shouldFire = latest.Count >= policy.FailThreshold && latest.All(x => !x.IsSuccess);
        if (shouldFire)
        {
            await FireOrUpdateAsync(
                firing,
                monitor,
                latestResult,
                RuleType.LinkDown,
                SeverityLevel.P1,
                BuildLinkDownMessage(latest.Count),
                dedupKey,
                notifyOnSeverityUpgradeOnly: true,
                ct);
            return;
        }

        if (latestResult.IsSuccess)
        {
            await ResolveIfNeededAsync(firing, monitor, latestResult, ct);
        }
    }

    private async Task ProcessCertAsync(MonMonitor monitor, MonPolicy policy, MonCheckResult latestResult, CancellationToken ct)
    {
        var expireThresholds = AlertRuleEvaluator.ParseThresholds(policy.CertExpireDaysThresholdsJson);
        var daysLeft = latestResult.CertDaysLeft ?? int.MaxValue;
        var isInvalid = !latestResult.IsSuccess;
        var shouldExpire = latestResult.IsSuccess && daysLeft <= expireThresholds.Max;

        var invalidKey = BuildDedupKey(monitor.Id, RuleType.CertInvalid);
        var expireKey = BuildDedupKey(monitor.Id, RuleType.CertExpire);
        var invalidFiring = await GetFiringAsync(invalidKey);
        var expireFiring = await GetFiringAsync(expireKey);

        if (isInvalid)
        {
            await FireOrUpdateAsync(
                invalidFiring,
                monitor,
                latestResult,
                RuleType.CertInvalid,
                AlertRuleEvaluator.ResolveCertSeverity(daysLeft, invalid: true),
                BuildCertInvalidMessage(latestResult.ErrorMessage),
                invalidKey,
                notifyOnSeverityUpgradeOnly: true,
                ct);

            await ResolveIfNeededAsync(expireFiring, monitor, latestResult, ct);
            return;
        }

        if (shouldExpire)
        {
            var severity = AlertRuleEvaluator.ResolveCertSeverity(daysLeft, invalid: false);
            await FireOrUpdateAsync(
                expireFiring,
                monitor,
                latestResult,
                RuleType.CertExpire,
                severity,
                BuildCertExpireMessage(daysLeft),
                expireKey,
                notifyOnSeverityUpgradeOnly: true,
                ct);

            await ResolveIfNeededAsync(invalidFiring, monitor, latestResult, ct);
            return;
        }

        await ResolveIfNeededAsync(invalidFiring, monitor, latestResult, ct);
        await ResolveIfNeededAsync(expireFiring, monitor, latestResult, ct);
    }

    private async Task FireOrUpdateAsync(
        AlertEvent? existing,
        MonMonitor monitor,
        MonCheckResult latestResult,
        string ruleType,
        string severity,
        string message,
        string dedupKey,
        bool notifyOnSeverityUpgradeOnly,
        CancellationToken ct)
    {
        if (existing is null)
        {
            var created = new AlertEvent
            {
                MonitorId = monitor.Id,
                RuleType = ruleType,
                Severity = severity,
                State = AlertState.Firing,
                FirstTriggeredAt = latestResult.CheckedAt,
                LastTriggeredAt = latestResult.CheckedAt,
                Message = message,
                DedupKey = dedupKey
            };
            created.Id = await _db.Insertable(created).ExecuteReturnIdentityAsync();
            await _notificationService.NotifyAsync(created, monitor, latestResult, ct);
            return;
        }

        var shouldNotify = !notifyOnSeverityUpgradeOnly ||
                           AlertRuleEvaluator.CompareSeverity(severity, existing.Severity) < 0;
        existing.LastTriggeredAt = latestResult.CheckedAt;
        existing.Message = message;
        existing.Severity = severity;
        await _db.Updateable(existing).ExecuteCommandAsync();
        if (shouldNotify)
        {
            await _notificationService.NotifyAsync(existing, monitor, latestResult, ct);
        }
    }

    private async Task ResolveIfNeededAsync(AlertEvent? firing, MonMonitor monitor, MonCheckResult latestResult, CancellationToken ct)
    {
        if (firing is null || !latestResult.IsSuccess)
        {
            return;
        }
        firing.State = AlertState.Resolved;
        firing.ResolvedAt = latestResult.CheckedAt;
        firing.LastTriggeredAt = latestResult.CheckedAt;
        await _db.Updateable(firing).ExecuteCommandAsync();
        await _notificationService.NotifyAsync(firing, monitor, latestResult, ct);
    }

    private Task<AlertEvent?> GetFiringAsync(string dedupKey)
    {
        return _db.Queryable<AlertEvent>()
            .Where(x => x.DedupKey == dedupKey && x.State == AlertState.Firing)
            .OrderByDescending(x => x.Id)
            .FirstAsync();
    }

    private static string BuildDedupKey(long monitorId, string ruleType) => $"{monitorId}:{ruleType}";

    private static string BuildLinkDownMessage(int count)
    {
        return $"链路连续 {count} 次探测失败。 / Link down for {count} consecutive checks.";
    }

    private static string BuildCertInvalidMessage(string? detail)
    {
        if (string.IsNullOrWhiteSpace(detail))
        {
            return "证书无效。 / Certificate invalid.";
        }

        return $"证书无效：{detail} / Certificate invalid: {detail}";
    }

    private static string BuildCertExpireMessage(int daysLeft)
    {
        return $"证书将在 {daysLeft} 天后过期。 / Certificate expires in {daysLeft} day(s).";
    }
}
