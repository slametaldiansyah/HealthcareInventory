using HealthcareScheduling.Application.DTOs;
using MediatR;

namespace HealthcareScheduling.Application.Features.Availability.Queries.GetDoctorAvailability;

public record GetDoctorAvailabilityQuery(
    Guid DoctorId,
    DateTimeOffset From,
    DateTimeOffset To,
    int SlotMinutes) : IRequest<IReadOnlyList<AvailabilitySlotDto>>;
