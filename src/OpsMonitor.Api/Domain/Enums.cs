namespace OpsMonitor.Api.Domain;

public static class MonitorType
{
    public const string Cert = "CERT";
    public const string Link = "LINK";
}

public static class AlertState
{
    public const string Firing = "FIRING";
    public const string Resolved = "RESOLVED";
}

public static class RuleType
{
    public const string LinkDown = "LINK_DOWN";
    public const string CertExpire = "CERT_EXPIRE";
    public const string CertInvalid = "CERT_INVALID";
}

public static class SeverityLevel
{
    public const string P1 = "P1";
    public const string P2 = "P2";
    public const string P3 = "P3";
}

public static class ErrorType
{
    public const string None = "NONE";
    public const string Dns = "DNS";
    public const string Tcp = "TCP";
    public const string Tls = "TLS";
    public const string Http = "HTTP";
    public const string Timeout = "TIMEOUT";
    public const string Cert = "CERT";
    public const string Internal = "INTERNAL";
}

public static class ChannelType
{
    public const string DingTalk = "DINGTALK";
}

public static class UserRole
{
    public const string Admin = "Admin";
    public const string User = "User";
}
