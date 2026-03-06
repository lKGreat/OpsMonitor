using Microsoft.AspNetCore.Http;
using OpsMonitor.Api.Localization;

namespace OpsMonitor.Tests;

public class LocalizationTests
{
    [Theory]
    [InlineData(null, LocaleResolver.ZhCn)]
    [InlineData("", LocaleResolver.ZhCn)]
    [InlineData("zh-CN", LocaleResolver.ZhCn)]
    [InlineData("zh-TW,zh;q=0.9", LocaleResolver.ZhCn)]
    [InlineData("en-US", LocaleResolver.EnUs)]
    [InlineData("en-GB,en;q=0.9", LocaleResolver.EnUs)]
    public void LocaleResolver_Resolve_Works(string? value, string expected)
    {
        var actual = LocaleResolver.Resolve(value);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TextLocalizer_ReturnsExpectedMessages()
    {
        var localizer = new TextLocalizer();

        var zh = localizer.Get(ErrorCodes.Auth.InvalidCredentials, LocaleResolver.ZhCn);
        var en = localizer.Get(ErrorCodes.Auth.InvalidCredentials, LocaleResolver.EnUs);

        Assert.Equal("用户名或密码错误，或账号已锁定。", zh);
        Assert.Equal("Invalid credentials or account locked.", en);
    }

    [Fact]
    public void ApiErrorFactory_UsesRequestLocale()
    {
        var localizer = new TextLocalizer();
        var factory = new ApiErrorFactory(localizer);
        var http = new DefaultHttpContext();
        http.Request.Headers.AcceptLanguage = "en-US";

        var error = factory.Create(http, ErrorCodes.Common.NotFound);

        Assert.Equal(ErrorCodes.Common.NotFound, error.Code);
        Assert.Equal("Resource not found.", error.Message);
    }
}
