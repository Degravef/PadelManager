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
        services.AddScoped<ICourtService, CourtService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IParticipationService, ParticipationService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IMatchLifecycleService, MatchLifecycleService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<ISiteScheduleService, SiteScheduleService>();

// Validation
        services.AddValidatorsFromAssemblyContaining<CreateSiteDtoValidator>();
        return services;
    }
}