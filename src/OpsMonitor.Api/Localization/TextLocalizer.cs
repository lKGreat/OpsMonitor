namespace OpsMonitor.Api.Localization;

public interface ITextLocalizer
{
    string Get(string code, string locale, params object[] args);
}

public class TextLocalizer : ITextLocalizer
{
    private static readonly Dictionary<string, (string ZhCn, string EnUs)> Messages = new()
    {
        [ErrorCodes.Common.InternalError] = ("服务器内部错误。", "Internal server error."),
        [ErrorCodes.Common.InvalidRequest] = ("请求参数无效。", "Invalid request payload."),
        [ErrorCodes.Common.NotFound] = ("资源不存在。", "Resource not found."),

        [ErrorCodes.Auth.InvalidCredentials] = ("用户名或密码错误，或账号已锁定。", "Invalid credentials or account locked."),
        [ErrorCodes.Auth.Unauthorized] = ("未授权，请重新登录。", "Unauthorized. Please sign in again."),
        [ErrorCodes.Auth.Forbidden] = ("没有权限执行该操作。", "Forbidden. You do not have permission."),

        [ErrorCodes.Monitor.NameRequired] = ("名称不能为空。", "Name is required."),
        [ErrorCodes.Monitor.TypeInvalid] = ("类型必须是 LINK 或 CERT。", "Type must be LINK or CERT."),
        [ErrorCodes.Monitor.TargetRequired] = ("目标 UrlOrHost 不能为空。", "Target UrlOrHost is required."),
        [ErrorCodes.Monitor.IntervalTooSmall] = ("IntervalSec 必须大于等于 10。", "IntervalSec must be >= 10."),
        [ErrorCodes.Monitor.TimeoutTooSmall] = ("TimeoutMs 必须大于等于 500。", "TimeoutMs must be >= 500."),
        [ErrorCodes.Monitor.FailThresholdTooSmall] = ("FailThreshold 必须大于等于 1。", "FailThreshold must be >= 1."),
        [ErrorCodes.Monitor.InvalidJson] = ("JSON 格式无效。", "Invalid JSON format."),
        [ErrorCodes.Monitor.NotFound] = ("监控项不存在。", "Monitor not found."),

        [ErrorCodes.Channel.TypeUnsupported] = ("仅支持 DINGTALK 渠道类型。", "Only DINGTALK is supported."),
        [ErrorCodes.Channel.NameRequired] = ("渠道名称不能为空。", "Channel name is required."),
        [ErrorCodes.Channel.WebhookRequired] = ("Webhook 不能为空。", "Webhook is required."),
        [ErrorCodes.Channel.NotFound] = ("通知渠道不存在。", "Channel not found."),

        [ErrorCodes.User.CredentialsRequired] = ("用户名和密码不能为空。", "UserName and Password are required."),
        [ErrorCodes.User.RoleInvalid] = ("角色必须是 Admin 或 User。", "Role must be Admin or User."),
        [ErrorCodes.User.UserNameExists] = ("用户名已存在。", "UserName already exists."),
        [ErrorCodes.User.NotFound] = ("用户不存在。", "User not found."),

        [ErrorCodes.Alert.NotFound] = ("告警不存在。", "Alert not found.")
    };

    public string Get(string code, string locale, params object[] args)
    {
        if (!Messages.TryGetValue(code, out var pair))
        {
            return code;
        }

        var template = locale == LocaleResolver.EnUs ? pair.EnUs : pair.ZhCn;
        return args.Length > 0 ? string.Format(template, args) : template;
    }
}
