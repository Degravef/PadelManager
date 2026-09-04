using Core.Domain.Exceptions;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Dal;

public class UnitOfWork(PadelDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in context.ChangeTracker.Entries<IConcurrencyToken>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.Version++;
        }

        try
        {
            return await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrentModificationException();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw PostgresConstraintTranslator.Translate(ex);
        }
    }
}
