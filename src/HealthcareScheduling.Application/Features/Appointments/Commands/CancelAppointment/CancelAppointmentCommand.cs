using MediatR;

namespace HealthcareScheduling.Application.Features.Appointments.Commands.CancelAppointment;

public record CancelAppointmentCommand(Guid AppointmentId) : IRequest<Unit>;
