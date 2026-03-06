using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace OpsMonitor.Tests.Integration;

public class TestApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath;

    public TestApiFactory()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"opsmonitor-test-{Guid.NewGuid():N}.db");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = $"Data Source={_dbPath}",
                ["Jwt:Issuer"] = "OpsMonitorTest",
                ["Jwt:Audience"] = "OpsMonitorTest",
                ["Jwt:SigningKey"] = "OpsMonitorTestSigningKey_32_Chars!!",
                ["Security:ConfigEncryptionKey"] = "OpsMonitorTestEncryptKey_32_Chars!",
                ["Seed:AdminUserName"] = "admin",
                ["Seed:AdminPassword"] = "ChangeMe123!"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Disable background workers for deterministic API smoke tests.
            services.RemoveAll<IHostedService>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try
        {
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
            if (File.Exists(_dbPath + "-wal"))
            {
                File.Delete(_dbPath + "-wal");
            }
            if (File.Exists(_dbPath + "-shm"))
            {
                File.Delete(_dbPath + "-shm");
            }
        }
        catch
        {
            // ignore cleanup failures
        }
    }
}
