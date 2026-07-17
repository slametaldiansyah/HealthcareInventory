using HealthcareScheduling.Application.DTOs;
using HealthcareScheduling.Application.Services;
using HealthcareScheduling.Domain.Exceptions;
using HealthcareScheduling.Domain.Interfaces;
using MediatR;

namespace HealthcareScheduling.Application.Features.Availability.Queries.GetDoctorAvailability;

public class GetDoctorAvailabilityQueryHandler : IRequestHandler<GetDoctorAvailabilityQuery, IReadOnlyList<AvailabilitySlotDto>>
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public GetDoctorAvailabilityQueryHandler(
        IDoctorRepository doctorRepository,
        IAppointmentRepository appointmentRepository)
    {
        _doctorRepository = doctorRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<IReadOnlyList<AvailabilitySlotDto>> Handle(
        GetDoctorAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdWithScheduleAsync(request.DoctorId, cancellationToken)
            ?? throw new NotFoundException($"Doctor '{request.DoctorId}' was not found.");

        var appointments = await _appointmentRepository.GetActiveByDoctorInRangeAsync(
            request.DoctorId,
            request.From.UtcDateTime,
            request.To.UtcDateTime,
            cancellationToken);

        var slots = AppointmentSchedulingService.GenerateAvailableSlots(
            request.From,
            request.To,
            request.SlotMinutes,
            doctor.TimeZoneId,
            doctor.WorkingSchedules,
            appointments);

        return slots
            .Select(s => new AvailabilitySlotDto(s, s.AddMinutes(request.SlotMinutes)))
            .ToList();
    }
}
