using OpsMonitor.Api.Domain;
using OpsMonitor.Api.Services;
using SqlSugar;

namespace OpsMonitor.Api.HostedServices;

public class ProbeWorkerHostedService : BackgroundService
{
    private readonly ISqlSugarClient _db;
    private readonly IProbeDispatchQueue _queue;
    private readonly IProbeService _probeService;
    private readonly IAlertEngineService _alertEngineService;
    private readonly ILogger<ProbeWorkerHostedService> _logger;

    public ProbeWorkerHostedService(
        ISqlSugarClient db,
        IProbeDispatchQueue queue,
        IProbeService probeService,
        IAlertEngineService alertEngineService,
        ILogger<ProbeWorkerHostedService> logger)
    {
        _db = db;
        _queue = queue;
        _probeService = probeService;
        _alertEngineService = alertEngineService;
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
        var monitor = await _db.Queryable<MonMonitor>().InSingleAsync(monitorId);
        if (monitor is null || !monitor.IsEnabled)
        {
            return;
        }

        var target = await _db.Queryable<MonTarget>().FirstAsync(x => x.MonitorId == monitorId);
        var policy = await _db.Queryable<MonPolicy>().InSingleAsync(monitorId);
        if (target is null || policy is null)
        {
            return;
        }

        ProbeOutcome? finalOutcome = null;
        var retries = Math.Max(0, policy.RetryCount);
        for (var attempt = 0; attempt <= retries; attempt++)
        {
            finalOutcome = monitor.Type == MonitorType.Cert
                ? await _probeService.RunCertProbeAsync(target, policy, ct)
                : await _probeService.RunLinkProbeAsync(target, policy, ct);

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
        result.Id = await _db.Insertable(result).ExecuteReturnIdentityAsync();

        await _alertEngineService.ProcessAsync(monitor, policy, result, ct);
    }
}
