using HealthcareScheduling.Domain.Enums;

namespace HealthcareScheduling.Application.DTOs;

public record UserDto(
    Guid Id,
    string Name,
    string Email,
    UserRole Role,
    UserAccountStatus Status,
    Guid? DoctorId = null);
