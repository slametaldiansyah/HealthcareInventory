using FluentAssertions;
using HealthcareScheduling.Application.Services;

namespace HealthcareScheduling.UnitTests;

public class SchedulingRulesTests
{
    [Theory]
    [InlineData("2026-07-20T09:30:00Z", "2026-07-20T10:00:00Z", "2026-07-20T09:45:00Z", "2026-07-20T10:15:00Z", true)]
    [InlineData("2026-07-20T09:30:00Z", "2026-07-20T10:00:00Z", "2026-07-20T10:00:00Z", "2026-07-20T10:30:00Z", false)]
    [InlineData("2026-07-20T09:30:00Z", "2026-07-20T10:00:00Z", "2026-07-20T09:00:00Z", "2026-07-20T09:30:00Z", false)]
    public void HasOverlap_DetectsRangeConflicts(
        string startA,
        string endA,
        string startB,
        string endB,
        bool expected)
    {
        var result = SchedulingRules.HasOverlap(
            DateTime.Parse(startA).ToUniversalTime(),
            DateTime.Parse(endA).ToUniversalTime(),
            DateTime.Parse(startB).ToUniversalTime(),
            DateTime.Parse(endB).ToUniversalTime());

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("2026-07-20T07:30:00Z", "2026-07-20T10:00:00Z", true)]
    [InlineData("2026-07-20T09:00:00Z", "2026-07-20T10:00:00Z", false)]
    [InlineData("2026-07-20T08:00:00Z", "2026-07-20T10:00:00Z", true)]
    public void CanCancel_EnforcesTwoHourCutoff(string now, string appointmentStart, bool expected)
    {
        var result = SchedulingRules.CanCancel(
            DateTime.Parse(appointmentStart).ToUniversalTime(),
            DateTime.Parse(now).ToUniversalTime());

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("2026-07-20T09:02:00Z", "2026-07-20T09:00:00Z")]
    [InlineData("2026-07-20T09:07:00Z", "2026-07-20T09:05:00Z")]
    [InlineData("2026-07-20T09:08:00Z", "2026-07-20T09:10:00Z")]
    public void RoundToNearestFiveMinutesUtc_RoundsCorrectly(string input, string expected)
    {
        var result = SchedulingRules.RoundToNearestFiveMinutesUtc(DateTime.Parse(input).ToUniversalTime());

        result.Should().Be(DateTime.Parse(expected).ToUniversalTime());
    }

    [Theory]
    [InlineData(15, true)]
    [InlineData(30, true)]
    [InlineData(60, true)]
    [InlineData(45, false)]
    public void IsValidDuration_AcceptsOnlySupportedValues(int duration, bool expected)
    {
        SchedulingRules.IsValidDuration(duration).Should().Be(expected);
    }
}
