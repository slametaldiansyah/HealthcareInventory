namespace HealthcareScheduling.Application.DTOs;

public record CreateDoctorByAdminRequestDto(
    string Name,
    string Email,
    string Password,
    string TimeZoneId,
    IReadOnlyList<WorkingScheduleRequestDto> WorkingSchedules);
