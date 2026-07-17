using HealthcareScheduling.Application.DTOs;
using MediatR;

namespace HealthcareScheduling.Application.Features.Admin.Queries.GetPendingRegistrations;

public record GetPendingRegistrationsQuery : IRequest<IReadOnlyList<PendingRegistrationDto>>;
