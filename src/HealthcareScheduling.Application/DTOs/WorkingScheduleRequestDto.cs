namespace HealthcareScheduling.Application.DTOs;

public record WorkingScheduleRequestDto(
    DayOfWeek DayOfWeek,
    string StartTime,
    string EndTime);
