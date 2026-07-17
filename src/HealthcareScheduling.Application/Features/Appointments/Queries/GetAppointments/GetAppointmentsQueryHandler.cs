using AutoMapper;
using HealthcareScheduling.Application.Common.Interfaces;
using HealthcareScheduling.Application.DTOs;
using HealthcareScheduling.Domain.Enums;
using HealthcareScheduling.Domain.Exceptions;
using HealthcareScheduling.Domain.Interfaces;
using MediatR;

namespace HealthcareScheduling.Application.Features.Appointments.Queries.GetAppointments;

public class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, IReadOnlyList<AppointmentResponseDto>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public GetAppointmentsQueryHandler(
        IAppointmentRepository appointmentRepository,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<AppointmentResponseDto>> Handle(
        GetAppointmentsQuery request,
        CancellationToken cancellationToken)
    {
        var role = _currentUser.Role
            ?? throw new UnauthorizedAccessException("Authentication is required.");

        var appointments = role switch
        {
            UserRole.Admin => await _appointmentRepository.GetAllAsync(cancellationToken),
            UserRole.Doctor => await GetDoctorAppointmentsAsync(cancellationToken),
            UserRole.User => await GetPatientAppointmentsAsync(cancellationToken),
            _ => throw new ForbiddenException("You are not allowed to list appointments.")
        };

        return _mapper.Map<IReadOnlyList<AppointmentResponseDto>>(appointments);
    }

    private async Task<IReadOnlyList<Domain.Entities.Appointment>> GetDoctorAppointmentsAsync(
        CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.DoctorId
            ?? throw new ForbiddenException("Doctor profile is not linked to this account.");

        return await _appointmentRepository.GetByDoctorIdAsync(doctorId, cancellationToken);
    }

    private async Task<IReadOnlyList<Domain.Entities.Appointment>> GetPatientAppointmentsAsync(
        CancellationToken cancellationToken)
    {
        var patientId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Authentication is required.");

        return await _appointmentRepository.GetByPatientIdAsync(patientId, cancellationToken);
    }
}
