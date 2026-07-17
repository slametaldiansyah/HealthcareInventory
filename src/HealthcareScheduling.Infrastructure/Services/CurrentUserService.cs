using System.Security.Claims;
using HealthcareScheduling.Application.Common.Interfaces;
using HealthcareScheduling.Domain.Constants;
using HealthcareScheduling.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace HealthcareScheduling.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userId, out var id) ? id : null;
        }
    }

    public UserRole? Role
    {
        get
        {
            var role = _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.Role);

            return Enum.TryParse<UserRole>(role, out var parsed) ? parsed : null;
        }
    }

    public Guid? DoctorId
    {
        get
        {
            var doctorId = _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(CustomClaimTypes.DoctorId);

            return Guid.TryParse(doctorId, out var id) ? id : null;
        }
    }

    public bool IsInRole(UserRole role) => Role == role;
}
