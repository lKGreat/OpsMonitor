namespace OpsMonitor.Api.Localization;

public static class ErrorCodes
{
    public static class Common
    {
        public const string InternalError = "common.internal_error";
        public const string InvalidRequest = "common.invalid_request";
        public const string NotFound = "common.not_found";
    }

    public static class Auth
    {
        public const string InvalidCredentials = "auth.invalid_credentials";
        public const string Unauthorized = "auth.unauthorized";
        public const string Forbidden = "auth.forbidden";
    }

    public static class Monitor
    {
        public const string NameRequired = "monitor.name_required";
        public const string TypeInvalid = "monitor.type_invalid";
        public const string TargetRequired = "monitor.target_required";
        public const string IntervalTooSmall = "monitor.interval_too_small";
        public const string TimeoutTooSmall = "monitor.timeout_too_small";
        public const string FailThresholdTooSmall = "monitor.fail_threshold_too_small";
        public const string InvalidJson = "monitor.invalid_json";
        public const string NotFound = "monitor.not_found";
    }

    public static class Channel
    {
        public const string TypeUnsupported = "channel.type_unsupported";
        public const string NameRequired = "channel.name_required";
        public const string WebhookRequired = "channel.webhook_required";
        public const string NotFound = "channel.not_found";
    }

    public static class User
    {
        public const string CredentialsRequired = "user.credentials_required";
        public const string RoleInvalid = "user.role_invalid";
        public const string UserNameExists = "user.username_exists";
        public const string NotFound = "user.not_found";
    }

    public static class Alert
    {
        public const string NotFound = "alert.not_found";
    }
}
