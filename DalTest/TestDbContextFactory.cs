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
        
        
        context.Database.EnsureCreated();
        return context;
    }
}