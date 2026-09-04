using Bll.Extensions;
using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Core.Dtos;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using FluentValidation;

namespace Bll.Services;

public class SiteScheduleService(
    ISiteScheduleRepository siteScheduleRepository,
    ISiteRepository siteRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreateSiteScheduleDto> createValidator) : ISiteScheduleService
{
    public async Task<SiteScheduleDto> GetBySiteAndYearAsync(int adminId, int siteId, int year)
    {
        await GetOwnedSiteOrThrowAsync(adminId, siteId);

        SiteSchedule? schedule = await siteScheduleRepository.GetBySiteAndYearAsync(siteId, year);
        if (schedule is null)
            throw new SiteScheduleNotDefinedException(siteId, year);

        return ToDto(schedule);
    }

    public async Task<SiteScheduleDto> CreateAsync(int adminId, int siteId, CreateSiteScheduleDto dto)
    {
        await createValidator.ValidateOrThrowAsync(dto);
        await GetOwnedSiteOrThrowAsync(adminId, siteId);

        var schedule = new SiteSchedule
        {
            SiteId = siteId,
            Year = dto.Year,
            OpeningTime = dto.OpeningTime,
            ClosingTime = dto.ClosingTime,
            MatchPrice = dto.MatchPrice ?? 60m
        };
        await siteScheduleRepository.AddAsync(schedule);
        await unitOfWork.SaveChangesAsync();

        return ToDto(schedule);
    }

    private async Task GetOwnedSiteOrThrowAsync(int adminId, int siteId)
    {
        Site? site = await siteRepository.GetByIdAsync(siteId);
        if (site is null || site.AdminId != adminId)
            throw new SiteNotFoundException(siteId);
    }

    private static SiteScheduleDto ToDto(SiteSchedule s) => new(
        s.Id, s.SiteId, s.Year, s.OpeningTime, s.ClosingTime,
        s.MatchDurationMinutes, s.BreakMinutes, s.MatchPrice, s.RequiredPlayers);
}
