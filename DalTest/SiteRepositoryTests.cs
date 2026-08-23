using Core.Domain.Entities;
using Dal;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DalTest;

public class SiteRepositoryTests
{
    private PadelDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PadelDbContext>()
                      .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                      .Options;
        return new PadelDbContext(options);
    }

    [Fact]
    public async Task AddAsync_SavesSiteToDatabase()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new SiteRepository(context);
        var site = new Site { Name = "Site 1", Address = "Address 1" };

        // Act
        await repository.AddAsync(site);
        await repository.SaveChangesAsync();

        // Assert
        var savedSite = await context.Sites.FirstOrDefaultAsync(s => s.Name == "Site 1");
        Assert.NotNull(savedSite);
        Assert.Equal("Address 1", savedSite.Address);
    }

    [Fact]
    public async Task ExistsByNameAsync_ReturnsTrue_WhenSiteExists()
    {
        // Arrange
        using var context = CreateContext();
        var site = new Site { Name = "Existing Site", Address = "Address" };
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        var repository = new SiteRepository(context);

        // Act
        var result = await repository.ExistsByNameAsync("Existing Site");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllSites()
    {
        // Arrange
        using var context = CreateContext();
        context.Sites.AddRange(
            new Site { Name = "S1", Address = "A1" },
            new Site { Name = "S2", Address = "A2" }
        );
        await context.SaveChangesAsync();
        var repository = new SiteRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }
}