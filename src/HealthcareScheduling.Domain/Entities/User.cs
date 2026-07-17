using HealthcareScheduling.Domain.Enums;

namespace HealthcareScheduling.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public UserAccountStatus Status { get; set; } = UserAccountStatus.Pending;
    public string? VerificationCode { get; set; }
    public DateTime? VerificationCodeExpiresAt { get; set; }
    public Guid? DoctorId { get; set; }

    public Doctor? Doctor { get; set; }
}
