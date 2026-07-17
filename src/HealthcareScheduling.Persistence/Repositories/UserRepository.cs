using HealthcareScheduling.Domain.Entities;
using HealthcareScheduling.Domain.Enums;
using HealthcareScheduling.Domain.Interfaces;
using HealthcareScheduling.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HealthcareScheduling.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    public async Task<User?> GetPendingByVerificationCodeAsync(
        string code,
        CancellationToken cancellationToken = default) =>
        await _context.Users
            .FirstOrDefaultAsync(
                u => u.Status == UserAccountStatus.Pending && u.VerificationCode == code,
                cancellationToken);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public async Task<bool> ExistsPendingVerificationCodeAsync(
        string code,
        CancellationToken cancellationToken = default) =>
        await _context.Users.AnyAsync(
            u => u.Status == UserAccountStatus.Pending && u.VerificationCode == code,
            cancellationToken);

    public async Task<IReadOnlyList<User>> GetByStatusAsync(
        UserAccountStatus status,
        CancellationToken cancellationToken = default) =>
        await _context.Users
            .AsNoTracking()
            .Where(u => u.Status == status)
            .OrderByDescending(u => u.VerificationCodeExpiresAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }
}
