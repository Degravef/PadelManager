using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Core.Dtos;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using FluentValidation;

namespace Bll.Services;

public class SiteService(
    ISiteRepository siteRepository,
    IValidator<CreateSiteDto> createValidator,
    IValidator<UpdateSiteDto> updateValidator) : ISiteService
{
    public async Task<SiteDto?> GetSiteByIdAsync(int id)
    {
        var site = await siteRepository.GetByIdAsync(id);
        return site != null ? new SiteDto(site.Id, site.Name, site.Address) : null;
    }

    public async Task<IEnumerable<SiteDto>> GetAllSitesAsync()
    {
        var sites = await siteRepository.GetAllAsync();
        return sites.Select(s => new SiteDto(s.Id, s.Name, s.Address));
    }

    public async Task<SiteDto> CreateSiteAsync(CreateSiteDto dto)
    {
        await createValidator.ValidateAndThrowAsync(dto);

        if (await siteRepository.ExistsByNameAsync(dto.Name))
        {
            throw new ConflictException($"Site with name '{dto.Name}' already exists.");
        }

        var site = new Site
        {
            Name = dto.Name,
            Address = dto.Address
        };

        await siteRepository.AddAsync(site);
        await siteRepository.SaveChangesAsync();

        return new SiteDto(site.Id, site.Name, site.Address);
    }

    public async Task UpdateSiteAsync(int id, UpdateSiteDto dto)
    {
        await updateValidator.ValidateAndThrowAsync(dto);

        var site = await siteRepository.GetByIdAsync(id)
                   ?? throw new NotFoundException($"Site with ID {id} not found.");

        if (site.Name != dto.Name && await siteRepository.ExistsByNameAsync(dto.Name))
        {
            throw new ConflictException($"Site with name '{dto.Name}' already exists.");
        }

        site.Name = dto.Name;
        site.Address = dto.Address;

        siteRepository.Update(site);
        await siteRepository.SaveChangesAsync();
    }

    public async Task DeleteSiteAsync(int id)
    {
        var site = await siteRepository.GetByIdAsync(id)
                   ?? throw new NotFoundException($"Site with ID {id} not found.");

        siteRepository.Delete(site);
        await siteRepository.SaveChangesAsync();
    }
}