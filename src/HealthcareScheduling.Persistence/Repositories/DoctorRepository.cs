using HealthcareScheduling.Domain.Entities;
using HealthcareScheduling.Domain.Interfaces;
using HealthcareScheduling.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HealthcareScheduling.Persistence.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly AppDbContext _context;

    public DoctorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Doctor?> GetByIdWithScheduleAsync(Guid doctorId, CancellationToken cancellationToken = default) =>
        await _context.Doctors
            .Include(d => d.WorkingSchedules)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == doctorId, cancellationToken);

    public async Task<IReadOnlyList<Doctor>> GetAllWithSchedulesAsync(CancellationToken cancellationToken = default) =>
        await _context.Doctors
            .Include(d => d.WorkingSchedules)
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsAsync(Guid doctorId, CancellationToken cancellationToken = default) =>
        await _context.Doctors.AnyAsync(d => d.Id == doctorId, cancellationToken);

    public async Task AddAsync(Doctor doctor, CancellationToken cancellationToken = default)
    {
        await _context.Doctors.AddAsync(doctor, cancellationToken);
    }
}
