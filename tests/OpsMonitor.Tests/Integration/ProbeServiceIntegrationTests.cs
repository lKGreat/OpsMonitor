using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpsMonitor.Api.Domain;
using OpsMonitor.Api.Services;
using Xunit;

namespace OpsMonitor.Tests.Integration;

public class ProbeServiceIntegrationTests
{
    private static IProbeService CreateProbeService()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("probe");
        services.AddSingleton<IProbeService, ProbeService>();
        return services.BuildServiceProvider().GetRequiredService<IProbeService>();
    }

    [Fact]
    public async Task LinkProbe_ShouldReturnSuccess_OnHttp200()
    {
        using var server = await LocalHttpServer.StartAsync(async ctx =>
        {
            ctx.Response.StatusCode = 200;
            await using var writer = new StreamWriter(ctx.Response.OutputStream);
            await writer.WriteAsync("ok");
        });

        var probe = CreateProbeService();
        var result = await probe.RunLinkProbeAsync(
            new MonTarget { UrlOrHost = server.Url, Port = server.Port },
            new MonPolicy { TimeoutMs = 2000, SuccessCodeRule = "200-399" });

        Assert.True(result.IsSuccess);
        Assert.Equal(ErrorType.None, result.ErrorType);
        Assert.Equal(200, result.HttpStatusCode);
    }

    [Fact]
    public async Task LinkProbe_ShouldReturnHttpError_OnHttp500()
    {
        using var server = await LocalHttpServer.StartAsync(async ctx =>
        {
            ctx.Response.StatusCode = 500;
            await using var writer = new StreamWriter(ctx.Response.OutputStream);
            await writer.WriteAsync("error");
        });

        var probe = CreateProbeService();
        var result = await probe.RunLinkProbeAsync(
            new MonTarget { UrlOrHost = server.Url, Port = server.Port },
            new MonPolicy { TimeoutMs = 2000, SuccessCodeRule = "200-399" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Http, result.ErrorType);
        Assert.Equal(500, result.HttpStatusCode);
    }

    [Fact]
    public async Task LinkProbe_ShouldReturnTimeout_OnSlowResponse()
    {
        using var server = await LocalHttpServer.StartAsync(async ctx =>
        {
            await Task.Delay(1200);
            ctx.Response.StatusCode = 200;
            await using var writer = new StreamWriter(ctx.Response.OutputStream);
            await writer.WriteAsync("slow");
        });

        var probe = CreateProbeService();
        var result = await probe.RunLinkProbeAsync(
            new MonTarget { UrlOrHost = server.Url, Port = server.Port },
            new MonPolicy { TimeoutMs = 200, SuccessCodeRule = "200-399" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Timeout, result.ErrorType);
    }

    [Fact]
    public async Task LinkProbe_ShouldFailTls_OnHttpsToPlainTcp()
    {
        using var tcp = new TcpListener(IPAddress.Loopback, 0);
        tcp.Start();
        var port = ((IPEndPoint)tcp.LocalEndpoint).Port;
        var acceptTask = Task.Run(async () =>
        {
            using var client = await tcp.AcceptTcpClientAsync();
            await Task.Delay(500);
        });

        var probe = CreateProbeService();
        var result = await probe.RunLinkProbeAsync(
            new MonTarget { UrlOrHost = $"https://127.0.0.1:{port}/health", Port = port },
            new MonPolicy { TimeoutMs = 1200, SuccessCodeRule = "200-399" });

        Assert.False(result.IsSuccess);
        Assert.Contains(result.ErrorType, new[] { ErrorType.Tls, ErrorType.Internal, ErrorType.Http });
        await acceptTask;
    }

    private sealed class LocalHttpServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly Func<HttpListenerContext, Task> _handler;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _loopTask;

        public int Port { get; }
        public string Url => $"http://127.0.0.1:{Port}/health";

        private LocalHttpServer(HttpListener listener, int port, Func<HttpListenerContext, Task> handler)
        {
            _listener = listener;
            Port = port;
            _handler = handler;
            _loopTask = Task.Run(LoopAsync);
        }

        public static async Task<LocalHttpServer> StartAsync(Func<HttpListenerContext, Task> handler)
        {
            var port = GetFreePort();
            var listener = new HttpListener();
            listener.Prefixes.Add($"http://127.0.0.1:{port}/");
            listener.Start();
            await Task.Delay(50);
            return new LocalHttpServer(listener, port, handler);
        }

        private async Task LoopAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                HttpListenerContext? ctx = null;
                try
                {
                    ctx = await _listener.GetContextAsync();
                    await _handler(ctx);
                }
                catch when (_cts.IsCancellationRequested)
                {
                    break;
                }
                catch
                {
                    // ignore and continue serving next request
                }
                finally
                {
                    try { ctx?.Response.Close(); } catch { }
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            try { _listener.Stop(); } catch { }
            try { _listener.Close(); } catch { }
            try { _loopTask.Wait(TimeSpan.FromSeconds(1)); } catch { }
            _cts.Dispose();
        }

        private static int GetFreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
