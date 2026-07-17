using HealthcareScheduling.Application.DTOs;
using HealthcareScheduling.Domain.Enums;
using HealthcareScheduling.Domain.Interfaces;
using MediatR;

namespace HealthcareScheduling.Application.Features.Admin.Queries.GetPendingRegistrations;

public class GetPendingRegistrationsQueryHandler
    : IRequestHandler<GetPendingRegistrationsQuery, IReadOnlyList<PendingRegistrationDto>>
{
    private readonly IUserRepository _userRepository;

    public GetPendingRegistrationsQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<PendingRegistrationDto>> Handle(
        GetPendingRegistrationsQuery request,
        CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetByStatusAsync(UserAccountStatus.Pending, cancellationToken);

        return users
            .Select(u => new PendingRegistrationDto(
                u.Id,
                u.Name,
                u.Email,
                u.Status,
                u.VerificationCodeExpiresAt))
            .ToList();
    }
}
