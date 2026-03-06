using OpsMonitor.Api.Services;
using Xunit;

namespace OpsMonitor.Tests;

public class CertificateNameMatcherTests
{
    [Theory]
    [InlineData("api.example.com", "api.example.com", true)]
    [InlineData("*.example.com", "api.example.com", true)]
    [InlineData("*.example.com", "a.b.example.com", true)]
    [InlineData("*.example.com", "example.com", false)]
    [InlineData("api.example.com", "www.example.com", false)]
    public void WildcardMatchHost_ShouldMatchAsExpected(string pattern, string host, bool expected)
    {
        var actual = CertificateNameMatcher.WildcardMatchHost(pattern, host);
        Assert.Equal(expected, actual);
    }
}
