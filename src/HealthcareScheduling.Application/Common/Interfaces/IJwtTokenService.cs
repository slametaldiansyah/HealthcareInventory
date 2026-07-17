using HealthcareScheduling.Domain.Entities;

namespace HealthcareScheduling.Application.Common.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);
}
