namespace OpsMonitor.Api.Contracts;

public record AlertDto(
    long Id,
    long MonitorId,
    string RuleType,
    string Severity,
    string State,
    DateTime FirstTriggeredAt,
    DateTime LastTriggeredAt,
    DateTime? ResolvedAt,
    string Message,
    DateTime? AckedAt,
    string? AckedBy,
    string? AckNote);

public record AckAlertRequest(string? Note);
