using HealthcareScheduling.Application.DTOs;
using MediatR;

namespace HealthcareScheduling.Application.Features.Appointments.Commands.CreateAppointment;

public record CreateAppointmentCommand(
    Guid DoctorId,
    DateTimeOffset Start,
    int Duration,
    Guid? PatientId = null) : IRequest<AppointmentResponseDto>;
