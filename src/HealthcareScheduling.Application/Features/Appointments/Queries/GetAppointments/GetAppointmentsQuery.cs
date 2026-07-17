using HealthcareScheduling.Application.DTOs;
using MediatR;

namespace HealthcareScheduling.Application.Features.Appointments.Queries.GetAppointments;

public record GetAppointmentsQuery : IRequest<IReadOnlyList<AppointmentResponseDto>>;
