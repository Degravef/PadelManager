using Bll.Extensions;
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

    // No ownership scoping: any Member picking a court to book must be able to see
    // every site (CF-RV rules allow Global/Libre members to book at any site), not just
    // the calling admin's own — see DOMAIN_RULES.md §2.
    public async Task<IEnumerable<SiteDto>> GetAllSitesPublicAsync()
    {
        IEnumerable<Site> sites = await siteRepository.GetAllAsync();
        return sites.Select(ToDto);
    }

    public async Task<IEnumerable<int>> GetAllAdminIdsAsync()
    {
        return await siteRepository.GetDistinctAdminIdsAsync();
    }

    public async Task<SiteDto> CreateSiteAsync(int adminId, CreateSiteDto dto)
    {
        await createValidator.ValidateOrThrowAsync(dto);

        var site = new Site { Name = dto.Name, Address = dto.Address, AdminId = adminId };
        await siteRepository.AddAsync(site);
        await unitOfWork.SaveChangesAsync();

        return ToDto(site);
    }

    public async Task UpdateSiteAsync(int adminId, int id, UpdateSiteDto dto)
    {
        await updateValidator.ValidateOrThrowAsync(dto);

        var site = await GetOwnedSiteOrThrowAsync(adminId, id);
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
        if (site is null || site.AdminId != adminId)
            throw new SiteNotFoundException(id);
        return site;
    }

    private static SiteDto ToDto(Site site) => new(site.Id, site.Name, site.Address);
}