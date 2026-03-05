namespace OpsMonitor.Api.Options;

public class JwtOptions
{
    public string Issuer { get; set; } = "OpsMonitor";
    public string Audience { get; set; } = "OpsMonitor";
    public string SigningKey { get; set; } = string.Empty;
    public int ExpiresMinutes { get; set; } = 480;
}

public class SecurityOptions
{
    public string ConfigEncryptionKey { get; set; } = string.Empty;
}

public class SeedOptions
{
    public string AdminUserName { get; set; } = "admin";
    public string AdminPassword { get; set; } = "ChangeMe123!";
}

public class MonitoringOptions
{
    public int ResultRetentionDays { get; set; } = 30;
    public int SchedulerTickMs { get; set; } = 1000;
}
