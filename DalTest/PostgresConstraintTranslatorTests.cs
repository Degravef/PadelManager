using Core.Constants;
using Core.Domain.Exceptions;
using Dal;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DalTest;

public class PostgresConstraintTranslatorTests
{
    [Fact]
    public void Translate_SiteAdminIdNameViolation_ReturnsFriendlyConflictException()
    {
        var pg = CreateUniqueViolation(ConstraintsNames.SitesAdminIdName);
        var dbUpdateException = new DbUpdateException("Save failed", pg);

        var result = PostgresConstraintTranslator.Translate(dbUpdateException);

        var conflict = Assert.IsType<ConflictException>(result, exactMatch: false);
        Assert.Equal("Vous possédez déjà un site portant ce nom.", conflict.Message);
    }

    [Fact]
    public void Translate_UnknownUniqueConstraint_ReturnsGenericConflictException()
    {
        var pg = CreateUniqueViolation("UQ_SomeOtherTable_SomeColumn");
        var result = PostgresConstraintTranslator.Translate(new DbUpdateException("Save failed", pg));

        Assert.IsType<ConflictException>(result, exactMatch: false);
    }

    [Fact]
    public void Translate_ForeignKeyViolation_ReturnsOriginalException()
    {
        var pg = new PostgresException(
            messageText: "insert or update violates foreign key constraint",
            severity: "ERROR", invariantSeverity: "ERROR", sqlState: "23503"); // pas UniqueViolation
        var dbUpdateException = new DbUpdateException("Save failed", pg);

        Assert.Same(dbUpdateException, PostgresConstraintTranslator.Translate(dbUpdateException));
    }

    [Fact]
    public void Translate_NonPostgresInnerException_ReturnsOriginalException()
    {
        var dbUpdateException = new DbUpdateException("Save failed", new InvalidOperationException("boom"));

        Assert.Same(dbUpdateException, PostgresConstraintTranslator.Translate(dbUpdateException));
    }

    private static PostgresException CreateUniqueViolation(string constraintName) => new(
        messageText: "duplicate key value violates unique constraint",
        severity: "ERROR", invariantSeverity: "ERROR",
        sqlState: PostgresErrorCodes.UniqueViolation,
        constraintName: constraintName);
}