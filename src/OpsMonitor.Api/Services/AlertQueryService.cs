using OpsMonitor.Api.Contracts;
using OpsMonitor.Api.Domain;
using SqlSugar;

namespace OpsMonitor.Api.Services;

public interface IAlertQueryService
{
    Task<List<AlertDto>> QueryAsync(string? state, CancellationToken ct = default);
    Task<bool> AckAsync(long id, string userName, CancellationToken ct = default);
}

public class AlertQueryService : IAlertQueryService
{
    private static readonly DateTime LegacyNullDate = DateTime.UnixEpoch;
    private readonly ISqlSugarClient _db;

    public AlertQueryService(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<List<AlertDto>> QueryAsync(string? state, CancellationToken ct = default)
    {
        var query = _db.Queryable<AlertEvent>();
        if (!string.IsNullOrWhiteSpace(state))
        {
            query = query.Where(x => x.State == state);
        }

        var list = await query.OrderByDescending(x => x.Id).Take(1000).ToListAsync();
        return list.Select(x => new AlertDto(
            x.Id,
            x.MonitorId,
            x.RuleType,
            x.Severity,
            x.State,
            x.FirstTriggeredAt,
            x.LastTriggeredAt,
            NormalizeLegacyDate(x.ResolvedAt),
            x.Message,
            NormalizeLegacyDate(x.AckedAt),
            x.AckedBy)).ToList();
    }

    public async Task<bool> AckAsync(long id, string userName, CancellationToken ct = default)
    {
        var eventRow = await _db.Queryable<AlertEvent>().InSingleAsync(id);
        if (eventRow is null)
        {
            return false;
        }
        eventRow.AckedAt = DateTime.UtcNow;
        eventRow.AckedBy = userName;
        await _db.Updateable(eventRow).ExecuteCommandAsync();
        return true;
    }

    private static DateTime? NormalizeLegacyDate(DateTime? value)
    {
        return value.HasValue && value.Value <= LegacyNullDate ? null : value;
    }
}
