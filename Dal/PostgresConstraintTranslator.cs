using Core.Constants;
using Core.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Dal;

public static class PostgresConstraintTranslator
{
    public static Exception Translate(DbUpdateException exception)
    {
        if (exception.InnerException is not PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
            return exception;

        return pg.ConstraintName switch
        {
            ConstraintsNames.SitesAdminIdName => new SiteNameConflictException(),
            ConstraintsNames.TerrainsSiteIdName => new TerrainNameConflictException(),
            ConstraintsNames.MembresMatriculeName => new MembreMatriculeConflictException(),
            ConstraintsNames.ParticipationsMatchIdMembreIdName => new ParticipationConflictException(),
            ConstraintsNames.ParticipationsMatchIdNumeroPlaceName => new ParticipationConflictException(),
            ConstraintsNames.HorairesSitesSiteIdAnneeName => new HoraireSiteConflictException(),
            _ => new DuplicateRecordException()
        };
    }
}