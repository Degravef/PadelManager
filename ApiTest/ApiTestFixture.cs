using Dal;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiTest;

public class ApiTestFixture : IAsyncLifetime
{
    private const string TestConnectionString =
        "Host=localhost;Database=padelmanager_test";

    private WebApplicationFactory<Program> _factory = null!;
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = TestConnectionString
                })));

        Client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PadelDbContext>();
        await db.Database.EnsureDeletedAsync(); 
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _factory.DisposeAsync();
}