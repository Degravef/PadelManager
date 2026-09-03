using Bll.Services;
using Bll.Validators;
using Core.Interfaces.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Bll.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBllServices(this IServiceCollection services)
    {

        services.AddScoped<ISiteService, SiteService>();
        services.AddScoped<ITerrainService, TerrainService>();
        services.AddScoped<IMembreService, MembreService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IParticipationService, ParticipationService>();
        services.AddScoped<IPaiementService, PaiementService>();
        services.AddScoped<IMatchLifecycleService, MatchLifecycleService>();
        services.AddScoped<IStatistiquesService, StatistiquesService>();
        services.AddScoped<IHoraireSiteService, HoraireSiteService>();


        services.AddValidatorsFromAssemblyContaining<CreateSiteDtoValidator>();
        return services;
    }
}