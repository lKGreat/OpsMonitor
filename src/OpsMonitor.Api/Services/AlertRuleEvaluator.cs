using System.Text.Json;
using OpsMonitor.Api.Domain;

namespace OpsMonitor.Api.Services;

public static class AlertRuleEvaluator
{
    public static (int Min, int Max) ParseThresholds(string? json)
    {
        var defaults = new[] { 30, 15, 7, 3, 1 };
        try
        {
            var arr = JsonSerializer.Deserialize<int[]>(json ?? "[]");
            if (arr is { Length: > 0 })
            {
                return (arr.Min(), arr.Max());
            }
        }
        catch
        {
            // ignored
        }
        return (defaults.Min(), defaults.Max());
    }

    public static string ResolveCertSeverity(int daysLeft, bool invalid)
    {
        if (invalid || daysLeft <= 1) return SeverityLevel.P1;
        if (daysLeft <= 7) return SeverityLevel.P2;
        return SeverityLevel.P3;
    }

    public static int CompareSeverity(string a, string b)
    {
        static int Rank(string s) => s switch
        {
            SeverityLevel.P1 => 1,
            SeverityLevel.P2 => 2,
            _ => 3
        };
        return Rank(a).CompareTo(Rank(b));
    }
}
