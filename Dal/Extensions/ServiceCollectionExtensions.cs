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
        services.AddScoped<ICourtRepository, CourtRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IMemberTypeRepository, MemberTypeRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<IParticipationRepository, ParticipationRepository>();
        services.AddScoped<ISiteScheduleRepository, SiteScheduleRepository>();
        services.AddScoped<ISlotRepository, SlotRepository>();
        services.AddScoped<IClosureDayRepository, ClosureDayRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPenaltyRepository, PenaltyRepository>();
        services.AddScoped<IBalanceDueRepository, BalanceDueRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
