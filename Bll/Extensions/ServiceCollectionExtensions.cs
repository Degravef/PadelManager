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
// Services
        services.AddScoped<ISiteService, SiteService>();
        services.AddScoped<ITerrainService, TerrainService>();

// Validation
        services.AddValidatorsFromAssemblyContaining<CreateSiteDtoValidator>();
        return services;
    }
}