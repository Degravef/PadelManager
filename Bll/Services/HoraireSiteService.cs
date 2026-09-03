using Bll.Extensions;
using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Core.Dtos;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using FluentValidation;

namespace Bll.Services;

public class HoraireSiteService(
    IHoraireSiteRepository horaireSiteRepository,
    ISiteRepository siteRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreateHoraireSiteDto> createValidator) : IHoraireSiteService
{
    public async Task<HoraireSiteDto> GetBySiteAndYearAsync(int adminId, int siteId, int annee)
    {
        await GetOwnedSiteOrThrowAsync(adminId, siteId);

        HoraireSite? horaire = await horaireSiteRepository.GetBySiteAndYearAsync(siteId, annee);
        if (horaire is null)
            throw new HorairesSiteNonDefinisException(siteId, annee);

        return ToDto(horaire);
    }

    public async Task<HoraireSiteDto> CreateAsync(int adminId, int siteId, CreateHoraireSiteDto dto)
    {
        await createValidator.ValidateOrThrowAsync(dto);
        await GetOwnedSiteOrThrowAsync(adminId, siteId);

        var horaire = new HoraireSite
        {
            SiteId = siteId,
            Annee = dto.Annee,
            HeurePremiereReservation = dto.HeurePremiereReservation,
            HeureDerniereReservation = dto.HeureDerniereReservation,
            PrixMatch = dto.PrixMatch ?? 60m 
        };
        await horaireSiteRepository.AddAsync(horaire);
        await unitOfWork.SaveChangesAsync();

        return ToDto(horaire);
    }

    private async Task GetOwnedSiteOrThrowAsync(int adminId, int siteId)
    {
        Site? site = await siteRepository.GetByIdAsync(siteId);
        if (site is null || site.AdminId != adminId)
            throw new SiteNotFoundException(siteId);
    }

    private static HoraireSiteDto ToDto(HoraireSite h) => new(
        h.Id, h.SiteId, h.Annee, h.HeurePremiereReservation, h.HeureDerniereReservation,
        h.DureeMatchMinutes, h.PauseMinutes, h.PrixMatch);
}
