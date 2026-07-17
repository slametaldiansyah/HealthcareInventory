using System.Data;
using HealthcareScheduling.Domain.Entities;
using HealthcareScheduling.Domain.Enums;
using HealthcareScheduling.Domain.Exceptions;
using HealthcareScheduling.Domain.Interfaces;
using HealthcareScheduling.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HealthcareScheduling.Persistence.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly AppDbContext _context;

    public AppointmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(Guid appointmentId, CancellationToken cancellationToken = default) =>
        await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId, cancellationToken);

    public async Task<IReadOnlyList<Appointment>> GetActiveByDoctorInRangeAsync(
        Guid doctorId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default) =>
        await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId
                        && a.Status == AppointmentStatus.Active
                        && a.StartUtc < toUtc
                        && a.EndUtc > fromUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Appointment>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Appointments
            .AsNoTracking()
            .OrderByDescending(a => a.StartUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Appointment>> GetByDoctorIdAsync(
        Guid doctorId,
        CancellationToken cancellationToken = default) =>
        await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId)
            .OrderByDescending(a => a.StartUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Appointment>> GetByPatientIdAsync(
        Guid patientId,
        CancellationToken cancellationToken = default) =>
        await _context.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.StartUtc)
            .ToListAsync(cancellationToken);

    public async Task<Appointment> BookAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var overlapping = await _context.Appointments
                .Where(a => a.DoctorId == appointment.DoctorId
                            && a.Status == AppointmentStatus.Active
                            && a.StartUtc < appointment.EndUtc
                            && a.EndUtc > appointment.StartUtc)
                .ToListAsync(cancellationToken);

            if (overlapping.Count > 0)
            {
                throw new AppointmentConflictException(
                    "The requested appointment overlaps with an existing booking.");
            }

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return appointment;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new AppointmentConflictException(
                "The requested appointment could not be booked due to a concurrent update.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task CancelAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        appointment.Status = AppointmentStatus.Cancelled;
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
