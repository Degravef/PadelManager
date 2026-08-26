using Core.Constants;
using Core.Domain.Exceptions;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Dal;

public class UnitOfWork(PadelDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await context.SaveChangesAsync(ct);
        }
        catch ( DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
        {
            throw pg.ConstraintName switch
            {
                ConstraintsNames.SitesAdminIdName => new SiteNameConflictException(),
                _ => new DuplicateRecordException()
            };
        }
    }
}