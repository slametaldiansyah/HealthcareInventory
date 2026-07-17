using HealthcareScheduling.Domain.Enums;

namespace HealthcareScheduling.Application.DTOs;

public record PendingRegistrationDto(
    Guid Id,
    string Name,
    string Email,
    UserAccountStatus Status,
    DateTime? VerificationCodeExpiresAt);
