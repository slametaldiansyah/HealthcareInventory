using HealthcareScheduling.Domain.Entities;
using HealthcareScheduling.Domain.Enums;

namespace HealthcareScheduling.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<User?> GetPendingByVerificationCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ExistsPendingVerificationCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetByStatusAsync(
        UserAccountStatus status,
        CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);

    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}
