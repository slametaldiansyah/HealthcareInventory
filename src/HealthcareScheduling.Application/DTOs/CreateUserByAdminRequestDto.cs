namespace HealthcareScheduling.Application.DTOs;

public record CreateUserByAdminRequestDto(
    string Name,
    string Email,
    string Password,
    bool ActivateImmediately = true);
