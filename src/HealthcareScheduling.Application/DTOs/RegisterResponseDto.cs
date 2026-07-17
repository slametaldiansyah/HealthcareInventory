using HealthcareScheduling.Domain.Enums;

namespace HealthcareScheduling.Application.DTOs;

public record RegisterResponseDto(
    Guid UserId,
    string Email,
    UserAccountStatus Status,
    string VerificationCode);
