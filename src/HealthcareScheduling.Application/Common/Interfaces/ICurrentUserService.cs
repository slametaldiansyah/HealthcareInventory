using HealthcareScheduling.Domain.Enums;

namespace HealthcareScheduling.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    UserRole? Role { get; }
    Guid? DoctorId { get; }
    bool IsInRole(UserRole role);
}
