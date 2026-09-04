using Core.Domain.Exceptions;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Dal;

public class UnitOfWork(PadelDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Bump every modified IConcurrencyToken entity's Version before EF builds its UPDATE/DELETE
        // statements, so the WHERE clause carries the pre-change value — see IConcurrencyToken.
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
            // Another transaction already committed a change to one of these rows (payment vs.
            // daily batch, two concurrent payments, ...) — nothing here was persisted, so this is
            // a clean rollback; the caller/client is expected to retry with fresh data.
            throw new ConcurrentModificationException();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw PostgresConstraintTranslator.Translate(ex);
        }
    }
}