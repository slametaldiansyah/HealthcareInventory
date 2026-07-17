using AutoMapper;
using HealthcareScheduling.Application.Common.Interfaces;
using HealthcareScheduling.Application.DTOs;
using HealthcareScheduling.Application.Services;
using HealthcareScheduling.Domain.Entities;
using HealthcareScheduling.Domain.Enums;
using HealthcareScheduling.Domain.Exceptions;
using HealthcareScheduling.Domain.Interfaces;
using MediatR;

namespace HealthcareScheduling.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, AppointmentResponseDto>
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public CreateAppointmentCommandHandler(
        IDoctorRepository doctorRepository,
        IAppointmentRepository appointmentRepository,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _doctorRepository = doctorRepository;
        _appointmentRepository = appointmentRepository;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<AppointmentResponseDto> Handle(
        CreateAppointmentCommand request,
        CancellationToken cancellationToken)
    {
        var role = _currentUser.Role
            ?? throw new UnauthorizedAccessException("Authentication is required.");

        if (role == UserRole.Doctor)
        {
            throw new ForbiddenException("Doctors cannot create appointments.");
        }

        var patientId = ResolvePatientId(role, request.PatientId);

        var doctor = await _doctorRepository.GetByIdWithScheduleAsync(request.DoctorId, cancellationToken)
            ?? throw new NotFoundException($"Doctor '{request.DoctorId}' was not found.");

        var startUtc = SchedulingRules.RoundToNearestFiveMinutesUtc(request.Start.UtcDateTime);
        var endUtc = startUtc.AddMinutes(request.Duration);

        if (!SchedulingRules.IsAlignedToFiveMinutesUtc(startUtc))
        {
            throw new InvalidAppointmentException("Appointment start must align to 5-minute intervals.");
        }

        if (!AppointmentSchedulingService.FitsWithinWorkingHours(
                startUtc, endUtc, doctor.TimeZoneId, doctor.WorkingSchedules))
        {
            throw new InvalidAppointmentException("Appointment is outside the doctor's working hours.");
        }

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            DoctorId = request.DoctorId,
            PatientId = patientId,
            StartUtc = startUtc,
            EndUtc = endUtc
        };

        var created = await _appointmentRepository.BookAsync(appointment, cancellationToken);

        return _mapper.Map<AppointmentResponseDto>(created);
    }

    private Guid ResolvePatientId(UserRole role, Guid? requestPatientId)
    {
        if (role == UserRole.User)
        {
            return _currentUser.UserId
                ?? throw new UnauthorizedAccessException("Authentication is required.");
        }

        if (role == UserRole.Admin)
        {
            if (!requestPatientId.HasValue || requestPatientId.Value == Guid.Empty)
            {
                throw new InvalidAppointmentException("PatientId is required when an admin creates an appointment.");
            }

            return requestPatientId.Value;
        }

        throw new ForbiddenException("You are not allowed to create appointments.");
    }
}
