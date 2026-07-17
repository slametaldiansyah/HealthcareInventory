using HealthcareScheduling.Application.Common.Interfaces;

namespace HealthcareScheduling.Infrastructure.Auth;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string passwordHash) =>
        BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
