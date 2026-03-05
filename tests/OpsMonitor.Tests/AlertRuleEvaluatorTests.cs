using OpsMonitor.Api.Domain;
using OpsMonitor.Api.Services;

namespace OpsMonitor.Tests;

public class AlertRuleEvaluatorTests
{
    [Fact]
    public void ParseThresholds_ShouldUseCustomValues_WhenJsonValid()
    {
        var (min, max) = AlertRuleEvaluator.ParseThresholds("[20,10,5]");
        Assert.Equal(5, min);
        Assert.Equal(20, max);
    }

    [Fact]
    public void ResolveCertSeverity_ShouldReturnP1_WhenOneDayLeft()
    {
        var level = AlertRuleEvaluator.ResolveCertSeverity(1, invalid: false);
        Assert.Equal(SeverityLevel.P1, level);
    }

    [Fact]
    public void ResolveCertSeverity_ShouldReturnP2_WhenSevenDaysLeft()
    {
        var level = AlertRuleEvaluator.ResolveCertSeverity(7, invalid: false);
        Assert.Equal(SeverityLevel.P2, level);
    }

    [Fact]
    public void ResolveCertSeverity_ShouldReturnP1_WhenInvalid()
    {
        var level = AlertRuleEvaluator.ResolveCertSeverity(30, invalid: true);
        Assert.Equal(SeverityLevel.P1, level);
    }
}
