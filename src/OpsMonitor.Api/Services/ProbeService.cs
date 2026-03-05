using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using OpsMonitor.Api.Domain;

namespace OpsMonitor.Api.Services;

public class ProbeOutcome
{
    public bool IsSuccess { get; set; }
    public long DurationMs { get; set; }
    public string ErrorType { get; set; } = Domain.ErrorType.None;
    public string? ErrorMessage { get; set; }
    public int? HttpStatusCode { get; set; }
    public DateTime? CertNotAfter { get; set; }
    public int? CertDaysLeft { get; set; }
    public string? CertIssuer { get; set; }
    public string? CertSubject { get; set; }
    public string? CertFingerprint { get; set; }
    public string RawJson { get; set; } = "{}";
}

public interface IProbeService
{
    Task<ProbeOutcome> RunLinkProbeAsync(MonTarget target, MonPolicy policy, CancellationToken ct = default);
    Task<ProbeOutcome> RunCertProbeAsync(MonTarget target, MonPolicy policy, CancellationToken ct = default);
}

public class ProbeService : IProbeService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ProbeService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ProbeOutcome> RunLinkProbeAsync(MonTarget target, MonPolicy policy, CancellationToken ct = default)
    {
        var result = new ProbeOutcome();
        var phase = new Dictionary<string, long>();
        var sw = Stopwatch.StartNew();

        try
        {
            var url = BuildUrl(target);
            var uri = new Uri(url);
            var host = uri.Host;
            var port = uri.Port;
            var timeout = TimeSpan.FromMilliseconds(policy.TimeoutMs);

            var dnsSw = Stopwatch.StartNew();
            IPAddress[] addresses;
            using (var dnsCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
            {
                dnsCts.CancelAfter(timeout);
                addresses = await Dns.GetHostAddressesAsync(host, dnsCts.Token);
            }
            dnsSw.Stop();
            phase["dnsMs"] = dnsSw.ElapsedMilliseconds;
            if (addresses.Length == 0)
            {
                return Fail(result, sw, Domain.ErrorType.Dns, "No DNS address resolved.", phase);
            }

            using var tcpClient = new TcpClient();
            var tcpSw = Stopwatch.StartNew();
            using (var tcpCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
            {
                tcpCts.CancelAfter(timeout);
                await tcpClient.ConnectAsync(host, port, tcpCts.Token);
            }
            tcpSw.Stop();
            phase["tcpMs"] = tcpSw.ElapsedMilliseconds;

            if (uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                var tlsSw = Stopwatch.StartNew();
                using var networkStream = tcpClient.GetStream();
                using var sslStream = new SslStream(networkStream, false, (_, _, _, _) => true);
                using (var tlsCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
                {
                    tlsCts.CancelAfter(timeout);
                    await sslStream.AuthenticateAsClientAsync(
                        new SslClientAuthenticationOptions
                        {
                            TargetHost = host,
                            EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                            CertificateRevocationCheckMode = X509RevocationMode.NoCheck
                        },
                        tlsCts.Token);
                }
                tlsSw.Stop();
                phase["tlsMs"] = tlsSw.ElapsedMilliseconds;
            }

            var client = _httpClientFactory.CreateClient("probe");
            client.Timeout = timeout;
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await client.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            result.HttpStatusCode = (int)response.StatusCode;
            result.IsSuccess = IsStatusSuccess(policy.SuccessCodeRule, (int)response.StatusCode) &&
                               IsBodySuccess(policy.ContentContains, body);
            if (!result.IsSuccess)
            {
                result.ErrorType = Domain.ErrorType.Http;
                result.ErrorMessage = "Status or content rule not satisfied.";
            }
            result.DurationMs = sw.ElapsedMilliseconds;
            phase["totalMs"] = result.DurationMs;
            phase["responseBytes"] = body.Length;
            result.RawJson = JsonSerializer.Serialize(phase);
            return result;
        }
        catch (OperationCanceledException)
        {
            return Fail(result, sw, Domain.ErrorType.Timeout, "Probe timeout.", phase);
        }
        catch (SocketException ex)
        {
            return Fail(result, sw, Domain.ErrorType.Tcp, ex.Message, phase);
        }
        catch (AuthenticationException ex)
        {
            return Fail(result, sw, Domain.ErrorType.Tls, ex.Message, phase);
        }
        catch (HttpRequestException ex)
        {
            return Fail(result, sw, Domain.ErrorType.Http, ex.Message, phase);
        }
        catch (Exception ex)
        {
            return Fail(result, sw, Domain.ErrorType.Internal, ex.Message, phase);
        }
    }

    public async Task<ProbeOutcome> RunCertProbeAsync(MonTarget target, MonPolicy policy, CancellationToken ct = default)
    {
        var result = new ProbeOutcome();
        var phase = new Dictionary<string, object>();
        var sw = Stopwatch.StartNew();
        var host = target.UrlOrHost.Trim();
        var port = target.Port <= 0 ? 443 : target.Port;
        var timeout = TimeSpan.FromMilliseconds(policy.TimeoutMs);

        try
        {
            using var tcpClient = new TcpClient();
            using (var tcpCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
            {
                tcpCts.CancelAfter(timeout);
                await tcpClient.ConnectAsync(host, port, tcpCts.Token);
            }

            using var stream = tcpClient.GetStream();
            using var sslStream = new SslStream(stream, false, (_, _, _, _) => true);
            using (var tlsCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
            {
                tlsCts.CancelAfter(timeout);
                await sslStream.AuthenticateAsClientAsync(
                    new SslClientAuthenticationOptions
                    {
                        TargetHost = target.UseSni ? host : string.Empty,
                        EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                        CertificateRevocationCheckMode = X509RevocationMode.NoCheck
                    },
                    tlsCts.Token);
            }

            if (sslStream.RemoteCertificate is null)
            {
                return Fail(result, sw, Domain.ErrorType.Cert, "Remote certificate is missing.", phase);
            }

            var cert = new X509Certificate2(sslStream.RemoteCertificate);
            var now = DateTime.UtcNow;
            var daysLeft = (int)Math.Floor((cert.NotAfter.ToUniversalTime() - now).TotalDays);
            var chain = new X509Chain
            {
                ChainPolicy =
                {
                    RevocationMode = X509RevocationMode.NoCheck,
                    VerificationFlags = X509VerificationFlags.NoFlag
                }
            };
            var chainOk = chain.Build(cert);
            var hostOk = CertificateNameMatcher.MatchHost(cert, host);
            var notBeforeOk = cert.NotBefore.ToUniversalTime() <= now;
            var notAfterOk = cert.NotAfter.ToUniversalTime() > now;

            result.CertNotAfter = cert.NotAfter.ToUniversalTime();
            result.CertDaysLeft = daysLeft;
            result.CertIssuer = cert.Issuer;
            result.CertSubject = cert.Subject;
            result.CertFingerprint = cert.Thumbprint;

            var errors = new List<string>();
            if (!notBeforeOk) errors.Add("CERT_NOT_EFFECTIVE");
            if (!notAfterOk) errors.Add("CERT_EXPIRED");
            if (!hostOk) errors.Add("CERT_HOSTNAME_MISMATCH");
            if (!chainOk) errors.Add("CERT_CHAIN_INCOMPLETE");

            result.IsSuccess = errors.Count == 0;
            result.ErrorType = result.IsSuccess ? Domain.ErrorType.None : Domain.ErrorType.Cert;
            result.ErrorMessage = result.IsSuccess ? null : string.Join(", ", errors);
            result.DurationMs = sw.ElapsedMilliseconds;

            phase["hostMatch"] = hostOk;
            phase["chainOk"] = chainOk;
            phase["chainStatus"] = chain.ChainStatus.Select(x => x.StatusInformation.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            phase["notBefore"] = cert.NotBefore.ToUniversalTime();
            phase["notAfter"] = cert.NotAfter.ToUniversalTime();
            phase["daysLeft"] = daysLeft;
            phase["totalMs"] = result.DurationMs;
            result.RawJson = JsonSerializer.Serialize(phase);
            return result;
        }
        catch (OperationCanceledException)
        {
            return Fail(result, sw, Domain.ErrorType.Timeout, "Probe timeout.", phase);
        }
        catch (SocketException ex)
        {
            return Fail(result, sw, Domain.ErrorType.Tcp, ex.Message, phase);
        }
        catch (AuthenticationException ex)
        {
            return Fail(result, sw, Domain.ErrorType.Tls, ex.Message, phase);
        }
        catch (Exception ex)
        {
            return Fail(result, sw, Domain.ErrorType.Cert, ex.Message, phase);
        }
    }

    private static ProbeOutcome Fail(ProbeOutcome result, Stopwatch sw, string errorType, string message, Dictionary<string, object> phase)
    {
        result.IsSuccess = false;
        result.ErrorType = errorType;
        result.ErrorMessage = message;
        result.DurationMs = sw.ElapsedMilliseconds;
        phase["totalMs"] = result.DurationMs;
        phase["error"] = message;
        result.RawJson = JsonSerializer.Serialize(phase);
        return result;
    }

    private static ProbeOutcome Fail(ProbeOutcome result, Stopwatch sw, string errorType, string message, Dictionary<string, long> phase)
    {
        result.IsSuccess = false;
        result.ErrorType = errorType;
        result.ErrorMessage = message;
        result.DurationMs = sw.ElapsedMilliseconds;
        phase["totalMs"] = result.DurationMs;
        result.RawJson = JsonSerializer.Serialize(phase);
        return result;
    }

    private static string BuildUrl(MonTarget target)
    {
        if (target.UrlOrHost.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            target.UrlOrHost.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return target.UrlOrHost;
        }
        var path = string.IsNullOrWhiteSpace(target.Path) ? string.Empty : target.Path;
        if (!string.IsNullOrEmpty(path) && !path.StartsWith('/'))
        {
            path = "/" + path;
        }
        return $"https://{target.UrlOrHost}:{target.Port}{path}";
    }

    private static bool IsStatusSuccess(string rule, int statusCode)
    {
        if (string.IsNullOrWhiteSpace(rule))
        {
            return statusCode is >= 200 and < 400;
        }

        var parts = rule.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            if (part.Contains('-'))
            {
                var range = part.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (range.Length == 2 &&
                    int.TryParse(range[0], out var start) &&
                    int.TryParse(range[1], out var end) &&
                    statusCode >= start &&
                    statusCode <= end)
                {
                    return true;
                }
            }
            else if (int.TryParse(part, out var code) && code == statusCode)
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsBodySuccess(string? requiredText, string body)
    {
        return string.IsNullOrWhiteSpace(requiredText) || body.Contains(requiredText, StringComparison.OrdinalIgnoreCase);
    }

}
