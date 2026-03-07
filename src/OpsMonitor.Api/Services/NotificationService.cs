using System.Text.Json;
using OpsMonitor.Api.Contracts;
using OpsMonitor.Api.Domain;
using SqlSugar;

namespace OpsMonitor.Api.Services;

public interface INotificationService
{
    Task NotifyAsync(AlertEvent alert, MonMonitor monitor, MonCheckResult result, CancellationToken ct = default);
}

public class NotificationService : INotificationService
{
    private static readonly DateTime LegacyNullDate = DateTime.UnixEpoch;
    private readonly ISqlSugarClient _db;
    private readonly IChannelService _channelService;
    private readonly IDingTalkNotifier _dingTalkNotifier;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        ISqlSugarClient db,
        IChannelService channelService,
        IDingTalkNotifier dingTalkNotifier,
        ILogger<NotificationService> logger)
    {
        _db = db;
        _channelService = channelService;
        _dingTalkNotifier = dingTalkNotifier;
        _logger = logger;
    }

    public async Task NotifyAsync(AlertEvent alert, MonMonitor monitor, MonCheckResult result, CancellationToken ct = default)
    {
        var policy = await _db.Queryable<MonPolicy>().InSingleAsync(monitor.Id);
        if (policy is null || string.IsNullOrWhiteSpace(policy.ChannelIdsJson))
        {
            return;
        }

        long[] channelIds;
        try
        {
            channelIds = JsonSerializer.Deserialize<long[]>(policy.ChannelIdsJson) ?? Array.Empty<long>();
        }
        catch
        {
            return;
        }
        if (channelIds.Length == 0)
        {
            return;
        }

        var channels = await _db.Queryable<NotifyChannel>()
            .Where(x => channelIds.Contains(x.Id) && x.IsEnabled)
            .ToListAsync();

        foreach (var channel in channels.Where(x => x.Type == ChannelType.DingTalk))
        {
            var config = _channelService.ReadDingTalkConfig(channel);
            var markdown = BuildMarkdown(alert, monitor, result);
            var title = BuildTitle(alert, monitor);
            var attempt = 0;
            (bool IsSuccess, string Response) send = (false, "N/A");

            while (attempt < 3 && !send.IsSuccess)
            {
                attempt++;
                send = await _dingTalkNotifier.SendMarkdownAsync(config, title, markdown, ct);
                if (!send.IsSuccess)
                {
                    var delay = TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt));
                    await Task.Delay(delay, ct);
                }
            }

            await _db.Insertable(new NotifyLog
            {
                AlertEventId = alert.Id,
                ChannelId = channel.Id,
                SentAt = DateTime.UtcNow,
                IsSuccess = send.IsSuccess,
                Response = send.Response,
                RetryCount = attempt - 1
            }).ExecuteCommandAsync();

            if (!send.IsSuccess)
            {
                _logger.LogWarning("DingTalk notify failed. alert={AlertId}, channel={ChannelId}", alert.Id, channel.Id);
            }
        }
    }

    private static string BuildTitle(AlertEvent alert, MonMonitor monitor)
    {
        var stateZh = alert.State == AlertState.Firing ? "触发中" : "已恢复";
        var stateEn = alert.State == AlertState.Firing ? "FIRING" : "RESOLVED";
        return $"[{alert.Severity}] {monitor.Name} {stateZh}/{stateEn}";
    }

    private static string BuildMarkdown(AlertEvent alert, MonMonitor monitor, MonCheckResult result)
    {
        var stateZh = alert.State == AlertState.Firing ? "触发中" : "已恢复";
        var stateEn = alert.State == AlertState.Firing ? "FIRING" : "RESOLVED";
        var errorReason = result.ErrorMessage ?? "-";
        var lines = new List<string>
        {
            $"### [{alert.Severity}] {monitor.Name}",
            "#### 中文",
            $"- 状态: **{stateZh}**",
            $"- 规则: `{alert.RuleType}`",
            $"- 首次触发: {alert.FirstTriggeredAt:yyyy-MM-dd HH:mm:ss} UTC",
            $"- 最近触发: {alert.LastTriggeredAt:yyyy-MM-dd HH:mm:ss} UTC",
            $"- 错误类型: `{result.ErrorType}`",
            $"- 错误原因: {errorReason}",
            $"- 耗时: {result.DurationMs} ms",
            string.Empty,
            "#### English",
            $"- State: **{stateEn}**",
            $"- Rule: `{alert.RuleType}`",
            $"- First triggered: {alert.FirstTriggeredAt:yyyy-MM-dd HH:mm:ss} UTC",
            $"- Last triggered: {alert.LastTriggeredAt:yyyy-MM-dd HH:mm:ss} UTC",
            $"- Error type: `{result.ErrorType}`",
            $"- Error reason: {errorReason}",
            $"- Duration: {result.DurationMs} ms"
        };

        var resolvedAt = NormalizeLegacyDate(alert.ResolvedAt);
        if (resolvedAt.HasValue)
        {
            var duration = resolvedAt.Value - alert.FirstTriggeredAt;
            lines.Add($"- 恢复时间: {resolvedAt:yyyy-MM-dd HH:mm:ss} UTC");
            lines.Add($"- 故障持续: {duration.TotalMinutes:F1} 分钟");
            lines.Add($"- Resolved at: {resolvedAt:yyyy-MM-dd HH:mm:ss} UTC");
            lines.Add($"- Outage duration: {duration.TotalMinutes:F1} minutes");
        }

        if (result.CertNotAfter.HasValue)
        {
            lines.Add($"- 证书到期: {result.CertNotAfter:yyyy-MM-dd HH:mm:ss} UTC");
            lines.Add($"- 剩余天数: {result.CertDaysLeft}");
            lines.Add($"- Certificate expires at: {result.CertNotAfter:yyyy-MM-dd HH:mm:ss} UTC");
            lines.Add($"- Days left: {result.CertDaysLeft}");
        }

        return string.Join("\n", lines);
    }

    private static DateTime? NormalizeLegacyDate(DateTime? value)
    {
        return value.HasValue && value.Value <= LegacyNullDate ? null : value;
    }
}
