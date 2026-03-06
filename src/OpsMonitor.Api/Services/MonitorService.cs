using System.Text.Json;
using OpsMonitor.Api.Contracts;
using OpsMonitor.Api.Domain;
using OpsMonitor.Api.Localization;
using SqlSugar;

namespace OpsMonitor.Api.Services;

public interface IMonitorService
{
    Task<List<MonitorListItemDto>> GetListAsync(CancellationToken ct = default);
    Task<MonitorDetailDto?> GetAsync(long id, CancellationToken ct = default);
    Task<long> CreateAsync(long userId, MonitorUpsertDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(long id, MonitorUpsertDto dto, CancellationToken ct = default);
    Task<bool> SetEnabledAsync(long id, bool enabled, CancellationToken ct = default);
    Task<List<CheckResultDto>> GetResultsAsync(long monitorId, int days, CancellationToken ct = default);
}

public class MonitorService : IMonitorService
{
    private readonly ISqlSugarClient _db;

    public MonitorService(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<List<MonitorListItemDto>> GetListAsync(CancellationToken ct = default)
    {
        var monitors = await _db.Queryable<MonMonitor>().OrderByDescending(x => x.UpdatedAt).ToListAsync();
        var monitorIds = monitors.Select(x => x.Id).ToList();

        var lastMap = new Dictionary<long, MonCheckResult>();
        if (monitorIds.Count > 0)
        {
            var latest = await _db.Queryable<MonCheckResult>()
                .Where(x => monitorIds.Contains(x.MonitorId))
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            foreach (var group in latest.GroupBy(x => x.MonitorId))
            {
                lastMap[group.Key] = group.First();
            }
        }

        return monitors.Select(m =>
        {
            lastMap.TryGetValue(m.Id, out var last);
            return new MonitorListItemDto(
                m.Id,
                m.Name,
                m.Type,
                m.GroupName,
                m.IsEnabled,
                m.UpdatedAt,
                last?.IsSuccess,
                last?.CheckedAt,
                last?.ErrorType);
        }).ToList();
    }

    public async Task<MonitorDetailDto?> GetAsync(long id, CancellationToken ct = default)
    {
        var monitor = await _db.Queryable<MonMonitor>().InSingleAsync(id);
        if (monitor is null)
        {
            return null;
        }

        var target = await _db.Queryable<MonTarget>().FirstAsync(x => x.MonitorId == id);
        var policy = await _db.Queryable<MonPolicy>().InSingleAsync(id);
        if (target is null || policy is null)
        {
            return null;
        }

        return new MonitorDetailDto(
            monitor.Id,
            monitor.Name,
            monitor.Type,
            monitor.GroupName,
            monitor.TagsJson,
            monitor.IsEnabled,
            new MonitorTargetDto
            {
                UrlOrHost = target.UrlOrHost,
                Port = target.Port,
                Path = target.Path,
                UseSni = target.UseSni,
                HeadersJson = target.HeadersJson
            },
            new MonitorPolicyDto
            {
                IntervalSec = policy.IntervalSec,
                TimeoutMs = policy.TimeoutMs,
                RetryCount = policy.RetryCount,
                FailThreshold = policy.FailThreshold,
                SuccessCodeRule = policy.SuccessCodeRule,
                ContentContains = policy.ContentContains,
                LatencyMsThreshold = policy.LatencyMsThreshold,
                CertExpireDaysThresholdsJson = policy.CertExpireDaysThresholdsJson,
                ChannelIdsJson = policy.ChannelIdsJson
            });
    }

    public async Task<long> CreateAsync(long userId, MonitorUpsertDto dto, CancellationToken ct = default)
    {
        Validate(dto);
        var now = DateTime.UtcNow;
        var monitor = new MonMonitor
        {
            Name = dto.Name.Trim(),
            Type = dto.Type.Trim().ToUpperInvariant(),
            GroupName = dto.GroupName,
            TagsJson = ValidateJsonOrNull(dto.TagsJson),
            IsEnabled = true,
            CreatedBy = userId,
            CreatedAt = now,
            UpdatedAt = now
        };

        var monitorId = await _db.Insertable(monitor).ExecuteReturnIdentityAsync();
        await _db.Insertable(new MonTarget
        {
            MonitorId = monitorId,
            UrlOrHost = dto.Target.UrlOrHost.Trim(),
            Port = dto.Target.Port,
            Path = dto.Target.Path,
            UseSni = dto.Target.UseSni,
            HeadersJson = ValidateJsonOrNull(dto.Target.HeadersJson)
        }).ExecuteCommandAsync();

        await _db.Insertable(new MonPolicy
        {
            MonitorId = monitorId,
            IntervalSec = dto.Policy.IntervalSec,
            TimeoutMs = dto.Policy.TimeoutMs,
            RetryCount = dto.Policy.RetryCount,
            FailThreshold = dto.Policy.FailThreshold,
            SuccessCodeRule = dto.Policy.SuccessCodeRule,
            ContentContains = dto.Policy.ContentContains,
            LatencyMsThreshold = dto.Policy.LatencyMsThreshold,
            CertExpireDaysThresholdsJson = dto.Policy.CertExpireDaysThresholdsJson,
            ChannelIdsJson = dto.Policy.ChannelIdsJson
        }).ExecuteCommandAsync();
        return monitorId;
    }

    public async Task<bool> UpdateAsync(long id, MonitorUpsertDto dto, CancellationToken ct = default)
    {
        Validate(dto);
        var monitor = await _db.Queryable<MonMonitor>().InSingleAsync(id);
        if (monitor is null)
        {
            return false;
        }

        monitor.Name = dto.Name.Trim();
        monitor.Type = dto.Type.Trim().ToUpperInvariant();
        monitor.GroupName = dto.GroupName;
        monitor.TagsJson = ValidateJsonOrNull(dto.TagsJson);
        monitor.UpdatedAt = DateTime.UtcNow;
        await _db.Updateable(monitor).ExecuteCommandAsync();

        await _db.Updateable<MonTarget>()
            .SetColumns(x => new MonTarget
            {
                UrlOrHost = dto.Target.UrlOrHost.Trim(),
                Port = dto.Target.Port,
                Path = dto.Target.Path,
                UseSni = dto.Target.UseSni,
                HeadersJson = ValidateJsonOrNull(dto.Target.HeadersJson)
            })
            .Where(x => x.MonitorId == id)
            .ExecuteCommandAsync();

        await _db.Updateable<MonPolicy>()
            .SetColumns(x => new MonPolicy
            {
                IntervalSec = dto.Policy.IntervalSec,
                TimeoutMs = dto.Policy.TimeoutMs,
                RetryCount = dto.Policy.RetryCount,
                FailThreshold = dto.Policy.FailThreshold,
                SuccessCodeRule = dto.Policy.SuccessCodeRule,
                ContentContains = dto.Policy.ContentContains,
                LatencyMsThreshold = dto.Policy.LatencyMsThreshold,
                CertExpireDaysThresholdsJson = dto.Policy.CertExpireDaysThresholdsJson,
                ChannelIdsJson = dto.Policy.ChannelIdsJson
            })
            .Where(x => x.MonitorId == id)
            .ExecuteCommandAsync();

        return true;
    }

    public async Task<bool> SetEnabledAsync(long id, bool enabled, CancellationToken ct = default)
    {
        var changed = await _db.Updateable<MonMonitor>()
            .SetColumns(x => new MonMonitor
            {
                IsEnabled = enabled,
                UpdatedAt = DateTime.UtcNow
            })
            .Where(x => x.Id == id)
            .ExecuteCommandAsync();
        return changed > 0;
    }

    public async Task<List<CheckResultDto>> GetResultsAsync(long monitorId, int days, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddDays(-Math.Abs(days <= 0 ? 7 : days));
        var list = await _db.Queryable<MonCheckResult>()
            .Where(x => x.MonitorId == monitorId && x.CheckedAt >= since)
            .OrderByDescending(x => x.CheckedAt)
            .Take(1000)
            .ToListAsync();

        return list.Select(x => new CheckResultDto(
            x.Id,
            x.CheckedAt,
            x.IsSuccess,
            x.DurationMs,
            x.ErrorType,
            x.ErrorMessage,
            x.HttpStatusCode,
            x.CertNotAfter,
            x.CertDaysLeft,
            x.CertIssuer,
            x.CertSubject,
            x.CertFingerprint)).ToList();
    }

    private static void Validate(MonitorUpsertDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ApiException(ErrorCodes.Monitor.NameRequired);
        }
        if (dto.Type is not (MonitorType.Link or MonitorType.Cert))
        {
            throw new ApiException(ErrorCodes.Monitor.TypeInvalid);
        }
        if (string.IsNullOrWhiteSpace(dto.Target.UrlOrHost))
        {
            throw new ApiException(ErrorCodes.Monitor.TargetRequired);
        }
        if (dto.Policy.IntervalSec < 10)
        {
            throw new ApiException(ErrorCodes.Monitor.IntervalTooSmall);
        }
        if (dto.Policy.TimeoutMs < 500)
        {
            throw new ApiException(ErrorCodes.Monitor.TimeoutTooSmall);
        }
        if (dto.Policy.FailThreshold < 1)
        {
            throw new ApiException(ErrorCodes.Monitor.FailThresholdTooSmall);
        }
    }

    private static string? ValidateJsonOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        try
        {
            _ = JsonDocument.Parse(value);
        }
        catch (JsonException)
        {
            throw new ApiException(ErrorCodes.Monitor.InvalidJson);
        }
        return value;
    }
}
