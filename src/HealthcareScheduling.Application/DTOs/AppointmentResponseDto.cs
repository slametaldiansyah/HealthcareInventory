using HealthcareScheduling.Domain.Enums;

namespace HealthcareScheduling.Application.DTOs;

public record AppointmentResponseDto(
    Guid Id,
    Guid DoctorId,
    Guid PatientId,
    DateTimeOffset Start,
    DateTimeOffset End,
    AppointmentStatus Status);
