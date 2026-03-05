namespace OpsMonitor.Api.Contracts;

public class MonitorUpsertDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public string? TagsJson { get; set; }
    public MonitorTargetDto Target { get; set; } = new();
    public MonitorPolicyDto Policy { get; set; } = new();
}

public class MonitorTargetDto
{
    public string UrlOrHost { get; set; } = string.Empty;
    public int Port { get; set; } = 443;
    public string? Path { get; set; }
    public bool UseSni { get; set; } = true;
    public string? HeadersJson { get; set; }
}

public class MonitorPolicyDto
{
    public int IntervalSec { get; set; } = 60;
    public int TimeoutMs { get; set; } = 5000;
    public int RetryCount { get; set; } = 1;
    public int FailThreshold { get; set; } = 3;
    public string SuccessCodeRule { get; set; } = "200-399";
    public string? ContentContains { get; set; }
    public int? LatencyMsThreshold { get; set; }
    public string? CertExpireDaysThresholdsJson { get; set; } = "[30,15,7,3,1]";
    public string? ChannelIdsJson { get; set; }
}

public record MonitorListItemDto(
    long Id,
    string Name,
    string Type,
    string? GroupName,
    bool IsEnabled,
    DateTime UpdatedAt,
    bool? LastIsSuccess,
    DateTime? LastCheckedAt,
    string? LastErrorType);

public record MonitorDetailDto(
    long Id,
    string Name,
    string Type,
    string? GroupName,
    string? TagsJson,
    bool IsEnabled,
    MonitorTargetDto Target,
    MonitorPolicyDto Policy);

public record CheckResultDto(
    long Id,
    DateTime CheckedAt,
    bool IsSuccess,
    long DurationMs,
    string ErrorType,
    string? ErrorMessage,
    int? HttpStatusCode,
    DateTime? CertNotAfter,
    int? CertDaysLeft,
    string? CertIssuer,
    string? CertSubject,
    string? CertFingerprint);
