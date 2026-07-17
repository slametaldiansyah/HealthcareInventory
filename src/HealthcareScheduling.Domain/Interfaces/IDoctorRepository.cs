using HealthcareScheduling.Domain.Entities;

namespace HealthcareScheduling.Domain.Interfaces;

public interface IDoctorRepository
{
    Task<Doctor?> GetByIdWithScheduleAsync(Guid doctorId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Doctor>> GetAllWithSchedulesAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid doctorId, CancellationToken cancellationToken = default);

    Task AddAsync(Doctor doctor, CancellationToken cancellationToken = default);
}
