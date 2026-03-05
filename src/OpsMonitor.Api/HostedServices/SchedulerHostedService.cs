using Microsoft.Extensions.Options;
using OpsMonitor.Api.Domain;
using OpsMonitor.Api.Options;
using OpsMonitor.Api.Services;
using SqlSugar;

namespace OpsMonitor.Api.HostedServices;

public class SchedulerHostedService : BackgroundService
{
    private readonly ISqlSugarClient _db;
    private readonly IProbeDispatchQueue _queue;
    private readonly ILogger<SchedulerHostedService> _logger;
    private readonly MonitoringOptions _options;
    private readonly Dictionary<long, DateTime> _nextRunMap = new();

    public SchedulerHostedService(
        ISqlSugarClient db,
        IProbeDispatchQueue queue,
        IOptions<MonitoringOptions> options,
        ILogger<SchedulerHostedService> logger)
    {
        _db = db;
        _queue = queue;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var monitors = await _db.Queryable<MonMonitor>()
                    .Where(x => x.IsEnabled)
                    .ToListAsync();
                var ids = monitors.Select(x => x.Id).ToList();
                if (ids.Count == 0)
                {
                    await Task.Delay(_options.SchedulerTickMs, stoppingToken);
                    continue;
                }
                var policies = await _db.Queryable<MonPolicy>()
                    .Where(x => ids.Contains(x.MonitorId))
                    .ToListAsync();
                var policyMap = policies.ToDictionary(x => x.MonitorId, x => x);

                foreach (var monitor in monitors)
                {
                    if (!policyMap.TryGetValue(monitor.Id, out var policy))
                    {
                        continue;
                    }

                    if (!_nextRunMap.TryGetValue(monitor.Id, out var nextRun))
                    {
                        nextRun = now;
                    }

                    if (nextRun <= now && _queue.Enqueue(new ProbeDispatch(monitor.Id)))
                    {
                        _nextRunMap[monitor.Id] = now.AddSeconds(policy.IntervalSec);
                    }
                }

                var staleIds = _nextRunMap.Keys.Except(monitors.Select(x => x.Id)).ToList();
                foreach (var id in staleIds)
                {
                    _nextRunMap.Remove(id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduler loop failed.");
            }

            await Task.Delay(_options.SchedulerTickMs, stoppingToken);
        }
    }
}
