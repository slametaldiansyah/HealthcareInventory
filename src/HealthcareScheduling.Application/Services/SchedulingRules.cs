namespace HealthcareScheduling.Application.Services;

public static class SchedulingRules
{
    public static readonly int[] ValidDurationsMinutes = [15, 30, 60];
    public static readonly TimeSpan CancellationCutoff = TimeSpan.FromHours(2);
    public static readonly TimeSpan RoundingInterval = TimeSpan.FromMinutes(5);

    public static bool IsValidDuration(int durationMinutes) =>
        ValidDurationsMinutes.Contains(durationMinutes);

    public static DateTime RoundToNearestFiveMinutesUtc(DateTime utc)
    {
        var normalized = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        var ticks = RoundingInterval.Ticks;
        var roundedTicks = (normalized.Ticks + ticks / 2) / ticks * ticks;
        return new DateTime(roundedTicks, DateTimeKind.Utc);
    }

    public static bool IsAlignedToFiveMinutesUtc(DateTime utc)
    {
        var normalized = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return normalized.Millisecond == 0
               && normalized.Second == 0
               && normalized.Minute % 5 == 0;
    }

    public static bool HasOverlap(DateTime startA, DateTime endA, DateTime startB, DateTime endB) =>
        startA < endB && endA > startB;

    public static bool CanCancel(DateTime appointmentStartUtc, DateTime utcNow) =>
        appointmentStartUtc - utcNow >= CancellationCutoff;
}
