using FluentAssertions;
using HealthcareScheduling.Application.Services;
using HealthcareScheduling.Domain.Entities;
using HealthcareScheduling.Domain.Enums;

namespace HealthcareScheduling.UnitTests;

public class AppointmentSchedulingServiceTests
{
    private static readonly Guid DoctorId = Guid.NewGuid();

    [Fact]
    public void GenerateAvailableSlots_ReturnsExpectedThirtyMinuteSlots()
    {
        var monday = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc);
        var schedules = new[]
        {
            new WorkingSchedule
            {
                DoctorId = DoctorId,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(12, 0)
            }
        };

        var slots = AppointmentSchedulingService.GenerateAvailableSlots(
            new DateTimeOffset(monday.AddHours(8)),
            new DateTimeOffset(monday.AddHours(13)),
            30,
            "UTC",
            schedules,
            []);

        slots.Select(s => s.UtcDateTime.ToString("HH:mm"))
            .Should()
            .Equal("09:00", "09:30", "10:00", "10:30", "11:00", "11:30");
    }

    [Fact]
    public void GenerateAvailableSlots_ExcludesOverlappingAppointments()
    {
        var monday = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc);
        var schedules = new[]
        {
            new WorkingSchedule
            {
                DoctorId = DoctorId,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(12, 0)
            }
        };

        var appointments = new[]
        {
            new Appointment
            {
                DoctorId = DoctorId,
                StartUtc = monday.AddHours(9).AddMinutes(30),
                EndUtc = monday.AddHours(10),
                Status = AppointmentStatus.Active
            }
        };

        var slots = AppointmentSchedulingService.GenerateAvailableSlots(
            new DateTimeOffset(monday.AddHours(8)),
            new DateTimeOffset(monday.AddHours(13)),
            30,
            "UTC",
            schedules,
            appointments);

        slots.Should().NotContain(s => s.UtcDateTime.Hour == 9 && s.UtcDateTime.Minute == 30);
    }

    [Fact]
    public void HasOverlapWithAny_DetectsConflictWithExistingAppointment()
    {
        var appointments = new[]
        {
            new Appointment
            {
                StartUtc = DateTime.Parse("2026-07-20T09:30:00Z").ToUniversalTime(),
                EndUtc = DateTime.Parse("2026-07-20T10:00:00Z").ToUniversalTime(),
                Status = AppointmentStatus.Active
            }
        };

        var result = AppointmentSchedulingService.HasOverlapWithAny(
            DateTime.Parse("2026-07-20T09:45:00Z").ToUniversalTime(),
            DateTime.Parse("2026-07-20T10:15:00Z").ToUniversalTime(),
            appointments);

        result.Should().BeTrue();
    }

    [Fact]
    public void FitsWithinWorkingHours_RejectsAppointmentOutsideSchedule()
    {
        var schedules = new[]
        {
            new WorkingSchedule
            {
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(12, 0)
            }
        };

        var result = AppointmentSchedulingService.FitsWithinWorkingHours(
            DateTime.Parse("2026-07-20T12:00:00Z").ToUniversalTime(),
            DateTime.Parse("2026-07-20T12:30:00Z").ToUniversalTime(),
            "UTC",
            schedules);

        result.Should().BeFalse();
    }
}
