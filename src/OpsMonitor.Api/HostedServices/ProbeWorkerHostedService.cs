using OpsMonitor.Api.Domain;
using OpsMonitor.Api.Services;
using SqlSugar;

namespace OpsMonitor.Api.HostedServices;

public class ProbeWorkerHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IProbeDispatchQueue _queue;
    private readonly ILogger<ProbeWorkerHostedService> _logger;

    public ProbeWorkerHostedService(
        IServiceScopeFactory scopeFactory,
        IProbeDispatchQueue queue,
        ILogger<ProbeWorkerHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var dispatch = await _queue.DequeueAsync(stoppingToken);
            try
            {
                await HandleAsync(dispatch.MonitorId, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Probe task failed. monitorId={MonitorId}", dispatch.MonitorId);
            }
        }
    }

    private async Task HandleAsync(long monitorId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
        var probeService = scope.ServiceProvider.GetRequiredService<IProbeService>();
        var alertEngineService = scope.ServiceProvider.GetRequiredService<IAlertEngineService>();

        var monitor = await db.Queryable<MonMonitor>().InSingleAsync(monitorId);
        if (monitor is null || !monitor.IsEnabled)
        {
            return;
        }

        var target = await db.Queryable<MonTarget>().FirstAsync(x => x.MonitorId == monitorId);
        var policy = await db.Queryable<MonPolicy>().InSingleAsync(monitorId);
        if (target is null || policy is null)
        {
            return;
        }

        ProbeOutcome? finalOutcome = null;
        var retries = Math.Max(0, policy.RetryCount);
        for (var attempt = 0; attempt <= retries; attempt++)
        {
            finalOutcome = monitor.Type == MonitorType.Cert
                ? await probeService.RunCertProbeAsync(target, policy, ct)
                : await probeService.RunLinkProbeAsync(target, policy, ct);

            if (finalOutcome.IsSuccess || attempt == retries)
            {
                break;
            }
            await Task.Delay(200, ct);
        }

        if (finalOutcome is null)
        {
            return;
        }

        var result = new MonCheckResult
        {
            MonitorId = monitor.Id,
            CheckedAt = DateTime.UtcNow,
            IsSuccess = finalOutcome.IsSuccess,
            DurationMs = finalOutcome.DurationMs,
            ErrorType = finalOutcome.ErrorType,
            ErrorMessage = finalOutcome.ErrorMessage,
            HttpStatusCode = finalOutcome.HttpStatusCode,
            CertNotAfter = finalOutcome.CertNotAfter,
            CertDaysLeft = finalOutcome.CertDaysLeft,
            CertIssuer = finalOutcome.CertIssuer,
            CertSubject = finalOutcome.CertSubject,
            CertFingerprint = finalOutcome.CertFingerprint,
            RawJson = finalOutcome.RawJson
        };
        result.Id = await db.Insertable(result).ExecuteReturnIdentityAsync();

        await alertEngineService.ProcessAsync(monitor, policy, result, ct);
    }
}
