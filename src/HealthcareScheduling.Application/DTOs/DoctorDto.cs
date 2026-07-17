namespace HealthcareScheduling.Application.DTOs;

public record DoctorDto(
    Guid Id,
    string Name,
    string TimeZoneId,
    IReadOnlyList<DoctorWorkingScheduleDto> WorkingSchedules);
