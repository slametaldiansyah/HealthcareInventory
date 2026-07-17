namespace HealthcareScheduling.Application.DTOs;

public record AuthResponseDto(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    UserDto User);
