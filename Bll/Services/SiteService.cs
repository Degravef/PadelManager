using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Core.Dtos;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using FluentValidation;

namespace Bll.Services;

public class SiteService(
    ISiteRepository siteRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreateSiteDto> createValidator,
    IValidator<UpdateSiteDto> updateValidator) : ISiteService
{
    public async Task<SiteDto> GetSiteByIdAsync(int adminId, int id)
    {
        Site site = await GetOwnedSiteOrThrowAsync(adminId, id);
        return ToDto(site);
    }

    public async Task<IEnumerable<SiteDto>> GetAllSitesAsync(int adminId)
    {
        IEnumerable<Site> sites = await siteRepository.GetByAdminIdAsync(adminId);
        return sites.Select(ToDto);
    }

    public async Task<SiteDto> CreateSiteAsync(int adminId, CreateSiteDto dto)
    {
        await createValidator.ValidateAndThrowAsync(dto);

        var site = new Site { Name = dto.Name, Address = dto.Address, AdminId = adminId };

        await siteRepository.AddAsync(site);
        await unitOfWork.SaveChangesAsync(); // ConflictException si (AdminId, Name) existe déjà

        return ToDto(site);
    }

    public async Task UpdateSiteAsync(int adminId, int id, UpdateSiteDto dto)
    {
        await updateValidator.ValidateAndThrowAsync(dto);

        Site site = await GetOwnedSiteOrThrowAsync(adminId, id);
        site.Name = dto.Name;
        site.Address = dto.Address;

        siteRepository.Update(site);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteSiteAsync(int adminId, int id)
    {
        Site site = await GetOwnedSiteOrThrowAsync(adminId, id);
        siteRepository.Delete(site);
        await unitOfWork.SaveChangesAsync();
    }

    private async Task<Site> GetOwnedSiteOrThrowAsync(int adminId, int id)
    {
        Site? site = await siteRepository.GetByIdAsync(id);
        // Même exception que le site n'existe pas ou appartienne à un autre admin :
        // on ne révèle pas l'existence du site d'un tiers.
        if (site is null || site.AdminId != adminId)
            throw new NotFoundException($"Site with ID {id} not found.");

        return site;
    }

    private static SiteDto ToDto(Site site) => new(site.Id, site.Name, site.Address);
}