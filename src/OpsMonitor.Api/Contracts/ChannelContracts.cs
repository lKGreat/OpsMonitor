namespace OpsMonitor.Api.Contracts;

public class DingTalkChannelConfigDto
{
    public string Webhook { get; set; } = string.Empty;
    public string? Secret { get; set; }
}

public class ChannelUpsertDto
{
    public string Type { get; set; } = "DINGTALK";
    public string Name { get; set; } = string.Empty;
    public DingTalkChannelConfigDto Config { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
}

public record ChannelDto(
    long Id,
    string Type,
    string Name,
    bool IsEnabled,
    string WebhookMasked,
    bool HasSecret);
