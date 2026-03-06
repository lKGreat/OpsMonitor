using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using OpsMonitor.Api.Contracts;
using OpsMonitor.Api.Domain;
using OpsMonitor.Api.Security;
using SqlSugar;

namespace OpsMonitor.Api.Services;

public interface IChannelService
{
    Task<List<ChannelDto>> GetAllAsync(CancellationToken ct = default);
    Task<long> CreateAsync(ChannelUpsertDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(long id, ChannelUpsertDto dto, CancellationToken ct = default);
    Task<NotifyChannel?> GetByIdAsync(long id, CancellationToken ct = default);
    DingTalkChannelConfigDto ReadDingTalkConfig(NotifyChannel channel);
}

public class ChannelService : IChannelService
{
    private readonly ISqlSugarClient _db;
    private readonly IConfigEncryptionService _encryptionService;

    public ChannelService(ISqlSugarClient db, IConfigEncryptionService encryptionService)
    {
        _db = db;
        _encryptionService = encryptionService;
    }

    public async Task<List<ChannelDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _db.Queryable<NotifyChannel>().OrderBy(x => x.Id).ToListAsync();
        return list.Select(x =>
        {
            var config = ReadDingTalkConfig(x);
            return new ChannelDto(
                x.Id,
                x.Type,
                x.Name,
                x.IsEnabled,
                MaskWebhook(config.Webhook),
                !string.IsNullOrWhiteSpace(config.Secret));
        }).ToList();
    }

    public async Task<long> CreateAsync(ChannelUpsertDto dto, CancellationToken ct = default)
    {
        Validate(dto);
        var encrypted = _encryptionService.Encrypt(JsonSerializer.Serialize(dto.Config));
        var id = await _db.Insertable(new NotifyChannel
        {
            Type = dto.Type,
            Name = dto.Name.Trim(),
            ConfigJson = encrypted,
            IsEnabled = dto.IsEnabled
        }).ExecuteReturnIdentityAsync();
        return id;
    }

    public async Task<bool> UpdateAsync(long id, ChannelUpsertDto dto, CancellationToken ct = default)
    {
        Validate(dto);
        var row = await _db.Queryable<NotifyChannel>().InSingleAsync(id);
        if (row is null)
        {
            return false;
        }
        row.Type = dto.Type;
        row.Name = dto.Name.Trim();
        row.ConfigJson = _encryptionService.Encrypt(JsonSerializer.Serialize(dto.Config));
        row.IsEnabled = dto.IsEnabled;
        await _db.Updateable(row).ExecuteCommandAsync();
        return true;
    }

    public async Task<NotifyChannel?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await _db.Queryable<NotifyChannel>().InSingleAsync(id);
    }

    public DingTalkChannelConfigDto ReadDingTalkConfig(NotifyChannel channel)
    {
        var json = _encryptionService.Decrypt(channel.ConfigJson);
        return JsonSerializer.Deserialize<DingTalkChannelConfigDto>(json) ?? new DingTalkChannelConfigDto();
    }

    private static void Validate(ChannelUpsertDto dto)
    {
        if (dto.Type != ChannelType.DingTalk)
        {
            throw new ArgumentException("Only DINGTALK is supported.");
        }
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException("Channel name is required.");
        }
        if (string.IsNullOrWhiteSpace(dto.Config.Webhook))
        {
            throw new ArgumentException("Webhook is required.");
        }
    }

    private static string MaskWebhook(string webhook)
    {
        if (string.IsNullOrWhiteSpace(webhook) || webhook.Length < 20)
        {
            return "********";
        }
        return $"{webhook[..12]}********{webhook[^6..]}";
    }
}

public interface IDingTalkNotifier
{
    Task<(bool IsSuccess, string Response)> SendMarkdownAsync(
        DingTalkChannelConfigDto config,
        string title,
        string markdown,
        CancellationToken ct = default);
}

public class DingTalkNotifier : IDingTalkNotifier
{
    private readonly HttpClient _httpClient;

    public DingTalkNotifier(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(bool IsSuccess, string Response)> SendMarkdownAsync(
        DingTalkChannelConfigDto config,
        string title,
        string markdown,
        CancellationToken ct = default)
    {
        var url = BuildSignedWebhook(config.Webhook, config.Secret);
        var payload = new
        {
            msgtype = "markdown",
            markdown = new
            {
                title,
                text = markdown
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, payload, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        return (response.IsSuccessStatusCode, body);
    }

    private static string BuildSignedWebhook(string webhook, string? secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            return webhook;
        }

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var signStr = $"{timestamp}\n{secret}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var signData = hmac.ComputeHash(Encoding.UTF8.GetBytes(signStr));
        var sign = Uri.EscapeDataString(Convert.ToBase64String(signData));
        var sep = webhook.Contains('?') ? "&" : "?";
        return $"{webhook}{sep}timestamp={timestamp}&sign={sign}";
    }
}
