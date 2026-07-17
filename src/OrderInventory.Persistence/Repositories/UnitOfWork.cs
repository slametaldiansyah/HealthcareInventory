using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrderInventory.Domain.Exceptions;
using OrderInventory.Domain.Interfaces;
using OrderInventory.Persistence.Context;

namespace OrderInventory.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly OrderInventoryDbContext _context;

    public UnitOfWork(OrderInventoryDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var details = string.Join("; ", ex.Entries.Select(e =>
                $"{e.Metadata.ClrType.Name}:{e.State}"));
            _context.ChangeTracker.Clear();
            throw new ConcurrencyConflictException(
                $"A concurrent update conflicted with this operation ({details}).", ex);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            _context.ChangeTracker.Clear();
            throw new DuplicateKeyException("A unique constraint was violated.", ex);
        }
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        if (_context.Database.CurrentTransaction is not null)
        {
            await action(cancellationToken);
            return;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);

        try
        {
            await action(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        if (exception.InnerException is SqlException sqlException)
        {
            // 2601: duplicate key in index; 2627: unique constraint violation
            return sqlException.Number is 2601 or 2627;
        }

        return false;
    }
}
