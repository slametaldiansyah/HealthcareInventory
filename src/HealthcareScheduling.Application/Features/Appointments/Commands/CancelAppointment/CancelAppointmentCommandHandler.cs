using HealthcareScheduling.Application.Common.Interfaces;
using HealthcareScheduling.Application.Services;
using HealthcareScheduling.Domain.Enums;
using HealthcareScheduling.Domain.Exceptions;
using HealthcareScheduling.Domain.Interfaces;
using MediatR;

namespace HealthcareScheduling.Application.Features.Appointments.Commands.CancelAppointment;

public class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, Unit>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserService _currentUser;

    public CancelAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUser)
    {
        _appointmentRepository = appointmentRepository;
        _dateTimeProvider = dateTimeProvider;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken)
            ?? throw new NotFoundException($"Appointment '{request.AppointmentId}' was not found.");

        EnsureCanCancel(appointment);

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            throw new CancellationNotAllowedException("Appointment is already cancelled.");
        }

        if (!SchedulingRules.CanCancel(appointment.StartUtc, _dateTimeProvider.UtcNow))
        {
            throw new CancellationNotAllowedException(
                "Cancellation is only allowed at least 2 hours before the appointment start.");
        }

        await _appointmentRepository.CancelAsync(appointment, cancellationToken);

        return Unit.Value;
    }

    private void EnsureCanCancel(Domain.Entities.Appointment appointment)
    {
        var role = _currentUser.Role
            ?? throw new UnauthorizedAccessException("Authentication is required.");

        switch (role)
        {
            case UserRole.Admin:
                return;
            case UserRole.User when _currentUser.UserId == appointment.PatientId:
                return;
            case UserRole.Doctor when _currentUser.DoctorId == appointment.DoctorId:
                return;
            default:
                throw new ForbiddenException("You are not allowed to cancel this appointment.");
        }
    }
}
