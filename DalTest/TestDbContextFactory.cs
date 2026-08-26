using Dal;
using Microsoft.EntityFrameworkCore;

namespace DalTest;

internal static class TestDbContextFactory
{
    public static PadelDbContext CreateInMemory(string? databaseName = null) =>
        new(new DbContextOptionsBuilder<PadelDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options);
}