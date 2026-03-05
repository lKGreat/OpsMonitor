using System.Security.Cryptography.X509Certificates;

namespace OpsMonitor.Api.Services;

public static class CertificateNameMatcher
{
    public static bool MatchHost(X509Certificate2 cert, string host)
    {
        var dnsNames = GetDnsNames(cert);
        if (dnsNames.Count == 0)
        {
            var cn = cert.GetNameInfo(X509NameType.DnsName, false);
            if (!string.IsNullOrWhiteSpace(cn))
            {
                dnsNames.Add(cn);
            }
        }
        return dnsNames.Any(pattern => WildcardMatchHost(pattern, host));
    }

    public static bool WildcardMatchHost(string pattern, string host)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return false;
        }
        if (string.Equals(pattern, host, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        if (pattern.StartsWith("*."))
        {
            var suffix = pattern[1..];
            return host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) &&
                   host.Count(c => c == '.') >= suffix.Count(c => c == '.');
        }
        return false;
    }

    private static List<string> GetDnsNames(X509Certificate2 cert)
    {
        var result = new List<string>();
        foreach (var extension in cert.Extensions)
        {
            if (extension.Oid?.Value != "2.5.29.17")
            {
                continue;
            }
            var formatted = extension.Format(false);
            var segments = formatted.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var segment in segments)
            {
                const string prefix = "DNS Name=";
                if (segment.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(segment[prefix.Length..].Trim());
                }
            }
        }
        return result;
    }
}
