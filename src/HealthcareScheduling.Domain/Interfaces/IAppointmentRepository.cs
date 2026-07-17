using HealthcareScheduling.Domain.Entities;

namespace HealthcareScheduling.Domain.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Appointment>> GetActiveByDoctorInRangeAsync(
        Guid doctorId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Appointment>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Appointment>> GetByDoctorIdAsync(
        Guid doctorId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Appointment>> GetByPatientIdAsync(
        Guid patientId,
        CancellationToken cancellationToken = default);

    Task<Appointment> BookAsync(Appointment appointment, CancellationToken cancellationToken = default);

    Task CancelAsync(Appointment appointment, CancellationToken cancellationToken = default);
}
