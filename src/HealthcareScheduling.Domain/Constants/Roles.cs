using HealthcareScheduling.Domain.Enums;

namespace HealthcareScheduling.Domain.Constants;

public static class Roles
{
    public const string Admin = nameof(UserRole.Admin);
    public const string User = nameof(UserRole.User);
    public const string Doctor = nameof(UserRole.Doctor);
}
