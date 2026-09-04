using Dal;
using Microsoft.EntityFrameworkCore;

namespace DalTest;

internal static class TestDbContextFactory
{
    public static PadelDbContext CreateInMemory(string? databaseName = null)
    {
        var context = new PadelDbContext(new DbContextOptionsBuilder<PadelDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options);
        // EnsureCreated (not just first use) is what actually applies HasData seeding
        // (see Dal/Configurations/MemberTypeConfiguration.cs) on the InMemory provider.
        context.Database.EnsureCreated();
        return context;
    }
}
