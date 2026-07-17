namespace HealthcareScheduling.Application.DTOs;

public record DoctorWorkingScheduleDto(
    DayOfWeek DayOfWeek,
    string StartTime,
    string EndTime);
