using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace OpsMonitor.Tests.Integration;

public class ApiSmokeTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;

    public ApiSmokeTests(TestApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_CreateChannel_CreateMonitor_QueryMonitors_ShouldSucceed()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            userName = "admin",
            password = "ChangeMe123!"
        });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        // First login requires password change; change it to continue protected APIs.
        var changePasswordResponse = await _client.PostAsJsonAsync("/api/auth/change-password", new
        {
            currentPassword = "ChangeMe123!",
            newPassword = "ChangeMe123!_Updated"
        });
        Assert.Equal(HttpStatusCode.OK, changePasswordResponse.StatusCode);

        var channelResp = await _client.PostAsJsonAsync("/api/channels", new
        {
            type = "DINGTALK",
            name = "test-channel",
            config = new
            {
                webhook = "https://oapi.dingtalk.com/robot/send?access_token=test",
                secret = "test-secret"
            },
            isEnabled = true
        });
        Assert.Equal(HttpStatusCode.OK, channelResp.StatusCode);
        var channelCreate = await channelResp.Content.ReadFromJsonAsync<IdPayload>();
        Assert.NotNull(channelCreate);

        var monitorResp = await _client.PostAsJsonAsync("/api/monitors", new
        {
            name = "smoke-link-monitor",
            type = "LINK",
            groupName = "smoke",
            target = new
            {
                urlOrHost = "http://127.0.0.1:65530/health",
                port = 65530,
                path = "/health",
                useSni = true
            },
            policy = new
            {
                intervalSec = 60,
                timeoutMs = 2000,
                retryCount = 0,
                failThreshold = 3,
                successCodeRule = "200-399",
                contentContains = "",
                certExpireDaysThresholdsJson = "[30,15,7,3,1]",
                channelIdsJson = $"[{channelCreate!.id}]"
            }
        });
        Assert.Equal(HttpStatusCode.OK, monitorResp.StatusCode);

        var monitorsResponse = await _client.GetAsync("/api/monitors");
        Assert.Equal(HttpStatusCode.OK, monitorsResponse.StatusCode);
        var monitors = await monitorsResponse.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();
        Assert.NotNull(monitors);
        Assert.NotEmpty(monitors!);
    }

    private class IdPayload
    {
        public long id { get; set; }
    }
}
