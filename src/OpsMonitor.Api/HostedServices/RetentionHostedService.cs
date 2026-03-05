using Microsoft.Extensions.Options;
using OpsMonitor.Api.Domain;
using OpsMonitor.Api.Options;
using SqlSugar;

namespace OpsMonitor.Api.HostedServices;

public class RetentionHostedService : BackgroundService
{
    private readonly ISqlSugarClient _db;
    private readonly MonitoringOptions _options;
    private readonly ILogger<RetentionHostedService> _logger;

    public RetentionHostedService(
        ISqlSugarClient db,
        IOptions<MonitoringOptions> options,
        ILogger<RetentionHostedService> logger)
    {
        _db = db;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var cutOff = DateTime.UtcNow.AddDays(-Math.Abs(_options.ResultRetentionDays));
                var deleted = await _db.Deleteable<MonCheckResult>()
                    .Where(x => x.CheckedAt < cutOff)
                    .ExecuteCommandAsync();
                if (deleted > 0)
                {
                    _logger.LogInformation("Retention cleaned {Count} check results before {CutOff}.", deleted, cutOff);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Retention cleanup failed.");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
