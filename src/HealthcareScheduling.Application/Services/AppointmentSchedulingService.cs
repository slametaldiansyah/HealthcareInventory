using HealthcareScheduling.Domain.Entities;
using HealthcareScheduling.Domain.Enums;
using TimeZoneConverter;

namespace HealthcareScheduling.Application.Services;

public static class AppointmentSchedulingService
{
    public static bool FitsWithinWorkingHours(
        DateTime startUtc,
        DateTime endUtc,
        string timeZoneId,
        IEnumerable<WorkingSchedule> schedules)
    {
        var timeZone = TZConvert.GetTimeZoneInfo(timeZoneId);
        var localStart = TimeZoneInfo.ConvertTimeFromUtc(startUtc, timeZone);
        var localEnd = TimeZoneInfo.ConvertTimeFromUtc(endUtc, timeZone);

        if (localStart.Date != localEnd.Date)
        {
            return false;
        }

        var daySchedule = schedules.FirstOrDefault(s => s.DayOfWeek == localStart.DayOfWeek);
        if (daySchedule is null)
        {
            return false;
        }

        var startTime = TimeOnly.FromDateTime(localStart);
        var endTime = TimeOnly.FromDateTime(localEnd);

        return startTime >= daySchedule.StartTime
               && endTime <= daySchedule.EndTime
               && SchedulingRules.IsAlignedToFiveMinutesUtc(startUtc);
    }

    public static bool HasOverlapWithAny(
        DateTime startUtc,
        DateTime endUtc,
        IEnumerable<Appointment> appointments) =>
        appointments
            .Where(a => a.Status == AppointmentStatus.Active)
            .Any(a => SchedulingRules.HasOverlap(startUtc, endUtc, a.StartUtc, a.EndUtc));

    public static IReadOnlyList<DateTimeOffset> GenerateAvailableSlots(
        DateTimeOffset from,
        DateTimeOffset to,
        int slotMinutes,
        string timeZoneId,
        IEnumerable<WorkingSchedule> schedules,
        IEnumerable<Appointment> appointments)
    {
        if (!SchedulingRules.IsValidDuration(slotMinutes))
        {
            return [];
        }

        var timeZone = TZConvert.GetTimeZoneInfo(timeZoneId);
        var fromUtc = from.UtcDateTime;
        var toUtc = to.UtcDateTime;
        var localFrom = TimeZoneInfo.ConvertTimeFromUtc(fromUtc, timeZone);
        var localTo = TimeZoneInfo.ConvertTimeFromUtc(toUtc, timeZone);
        var activeAppointments = appointments
            .Where(a => a.Status == AppointmentStatus.Active)
            .ToList();

        var slots = new List<DateTimeOffset>();

        for (var date = localFrom.Date; date <= localTo.Date; date = date.AddDays(1))
        {
            foreach (var schedule in schedules.Where(s => s.DayOfWeek == date.DayOfWeek))
            {
                var current = schedule.StartTime;

                while (current.AddMinutes(slotMinutes) <= schedule.EndTime)
                {
                    var localDateTime = DateTime.SpecifyKind(date.Add(current.ToTimeSpan()), DateTimeKind.Unspecified);
                    var slotStartUtc = TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);
                    var slotEndUtc = slotStartUtc.AddMinutes(slotMinutes);

                    if (slotStartUtc >= fromUtc && slotEndUtc <= toUtc
                        && !HasOverlapWithAny(slotStartUtc, slotEndUtc, activeAppointments))
                    {
                        slots.Add(new DateTimeOffset(slotStartUtc, TimeSpan.Zero));
                    }

                    current = current.AddMinutes(slotMinutes);
                }
            }
        }

        return slots;
    }
}
