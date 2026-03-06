using SqlSugar;

namespace OpsMonitor.Api.Domain;

[SugarTable("sys_user")]
public class SysUser
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }
    [SugarColumn(Length = 64, IsNullable = false)]
    public string UserName { get; set; } = string.Empty;
    [SugarColumn(Length = 64, IsNullable = false)]
    public string DisplayName { get; set; } = string.Empty;
    [SugarColumn(Length = 16, IsNullable = false)]
    public string Role { get; set; } = UserRole.User;
    [SugarColumn(Length = 256, IsNullable = false)]
    public string PasswordHash { get; set; } = string.Empty;
    [SugarColumn(Length = 128, IsNullable = false)]
    public string PasswordSalt { get; set; } = string.Empty;
    [SugarColumn(IsNullable = false)]
    public int PwdIterations { get; set; }
    [SugarColumn(IsNullable = false)]
    public bool IsEnabled { get; set; } = true;
    [SugarColumn(IsNullable = false)]
    public bool RequirePasswordChange { get; set; } = true;
    [SugarColumn(IsNullable = false)]
    public DateTime CreatedAt { get; set; }
    [SugarColumn(IsNullable = false)]
    public DateTime UpdatedAt { get; set; }
    [SugarColumn(IsNullable = true)]
    public DateTime? LastLoginAt { get; set; }
}

[SugarTable("sys_audit_log")]
public class SysAuditLog
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }
    [SugarColumn(IsNullable = false)]
    public DateTime CreatedAt { get; set; }
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? UserName { get; set; }
    [SugarColumn(Length = 8, IsNullable = true)]
    public string? Method { get; set; }
    [SugarColumn(Length = 200, IsNullable = true)]
    public string? Path { get; set; }
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? Ip { get; set; }
    [SugarColumn(Length = 16, IsNullable = false)]
    public string Status { get; set; } = "OK";
    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Message { get; set; }
}

[SugarTable("mon_monitor")]
public class MonMonitor
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }
    [SugarColumn(Length = 128, IsNullable = false)]
    public string Name { get; set; } = string.Empty;
    [SugarColumn(Length = 8, IsNullable = false)]
    public string Type { get; set; } = MonitorType.Link;
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? GroupName { get; set; }
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? TagsJson { get; set; }
    [SugarColumn(IsNullable = false)]
    public bool IsEnabled { get; set; } = true;
    [SugarColumn(IsNullable = false)]
    public long CreatedBy { get; set; }
    [SugarColumn(IsNullable = false)]
    public DateTime CreatedAt { get; set; }
    [SugarColumn(IsNullable = false)]
    public DateTime UpdatedAt { get; set; }
}

[SugarTable("mon_target")]
public class MonTarget
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }
    [SugarColumn(IsNullable = false)]
    public long MonitorId { get; set; }
    [SugarColumn(Length = 512, IsNullable = false)]
    public string UrlOrHost { get; set; } = string.Empty;
    [SugarColumn(IsNullable = false)]
    public int Port { get; set; } = 443;
    [SugarColumn(Length = 256, IsNullable = true)]
    public string? Path { get; set; }
    [SugarColumn(IsNullable = false)]
    public bool UseSni { get; set; } = true;
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? HeadersJson { get; set; }
}

[SugarTable("mon_policy")]
public class MonPolicy
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
    public long MonitorId { get; set; }
    [SugarColumn(IsNullable = false)]
    public int IntervalSec { get; set; } = 60;
    [SugarColumn(IsNullable = false)]
    public int TimeoutMs { get; set; } = 5000;
    [SugarColumn(IsNullable = false)]
    public int RetryCount { get; set; } = 1;
    [SugarColumn(IsNullable = false)]
    public int FailThreshold { get; set; } = 3;
    [SugarColumn(Length = 64, IsNullable = false)]
    public string SuccessCodeRule { get; set; } = "200-399";
    [SugarColumn(Length = 200, IsNullable = true)]
    public string? ContentContains { get; set; }
    [SugarColumn(IsNullable = true)]
    public int? LatencyMsThreshold { get; set; }
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? CertExpireDaysThresholdsJson { get; set; }
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? ChannelIdsJson { get; set; }
}

[SugarTable("mon_check_result")]
public class MonCheckResult
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }
    [SugarColumn(IsNullable = false)]
    public long MonitorId { get; set; }
    [SugarColumn(IsNullable = false)]
    public DateTime CheckedAt { get; set; }
    [SugarColumn(IsNullable = false)]
    public bool IsSuccess { get; set; }
    [SugarColumn(IsNullable = false)]
    public long DurationMs { get; set; }
    [SugarColumn(Length = 16, IsNullable = false)]
    public string ErrorType { get; set; } = Domain.ErrorType.None;
    [SugarColumn(Length = 500, IsNullable = true)]
    public string? ErrorMessage { get; set; }
    public int? HttpStatusCode { get; set; }
    public DateTime? CertNotAfter { get; set; }
    public int? CertDaysLeft { get; set; }
    [SugarColumn(Length = 200, IsNullable = true)]
    public string? CertIssuer { get; set; }
    [SugarColumn(Length = 200, IsNullable = true)]
    public string? CertSubject { get; set; }
    [SugarColumn(Length = 128, IsNullable = true)]
    public string? CertFingerprint { get; set; }
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? RawJson { get; set; }
}

[SugarTable("alert_event")]
public class AlertEvent
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }
    [SugarColumn(IsNullable = false)]
    public long MonitorId { get; set; }
    [SugarColumn(Length = 32, IsNullable = false)]
    public string RuleType { get; set; } = string.Empty;
    [SugarColumn(Length = 2, IsNullable = false)]
    public string Severity { get; set; } = SeverityLevel.P3;
    [SugarColumn(Length = 16, IsNullable = false)]
    public string State { get; set; } = AlertState.Firing;
    [SugarColumn(IsNullable = false)]
    public DateTime FirstTriggeredAt { get; set; }
    [SugarColumn(IsNullable = false)]
    public DateTime LastTriggeredAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    [SugarColumn(Length = 500, IsNullable = false)]
    public string Message { get; set; } = string.Empty;
    [SugarColumn(Length = 128, IsNullable = false)]
    public string DedupKey { get; set; } = string.Empty;
    public DateTime? SilencedUntil { get; set; }
    public DateTime? AckedAt { get; set; }
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? AckedBy { get; set; }
}

[SugarTable("notify_channel")]
public class NotifyChannel
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }
    [SugarColumn(Length = 16, IsNullable = false)]
    public string Type { get; set; } = ChannelType.DingTalk;
    [SugarColumn(Length = 64, IsNullable = false)]
    public string Name { get; set; } = string.Empty;
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
    public string ConfigJson { get; set; } = string.Empty;
    [SugarColumn(IsNullable = false)]
    public bool IsEnabled { get; set; } = true;
}

[SugarTable("notify_log")]
public class NotifyLog
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }
    [SugarColumn(IsNullable = false)]
    public long AlertEventId { get; set; }
    [SugarColumn(IsNullable = false)]
    public long ChannelId { get; set; }
    [SugarColumn(IsNullable = false)]
    public DateTime SentAt { get; set; }
    [SugarColumn(IsNullable = false)]
    public bool IsSuccess { get; set; }
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Response { get; set; }
    [SugarColumn(IsNullable = false)]
    public int RetryCount { get; set; }
}
