using Core.Interfaces;
using Core.Interfaces.Repositories;
using Dal.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Dal.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDalServices(this IServiceCollection services)
    {
        services.AddScoped<ISiteRepository, SiteRepository>();
        services.AddScoped<ITerrainRepository, TerrainRepository>();
        services.AddScoped<IMembreRepository, MembreRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<IParticipationRepository, ParticipationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}