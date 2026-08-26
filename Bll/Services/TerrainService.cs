using Bll.Extensions;
using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Core.Dtos;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using FluentValidation;

namespace Bll.Services;

public class TerrainService(
    ITerrainRepository terrainRepository,
    ISiteRepository siteRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreateTerrainDto> createValidator,
    IValidator<UpdateTerrainDto> updateValidator) : ITerrainService
{
    public async Task<TerrainDto> GetTerrainByIdAsync(int adminId, int id)
    {
        Terrain terrain = await GetOwnedTerrainOrThrowAsync(adminId, id);
        return ToDto(terrain);
    }

    public async Task<IEnumerable<TerrainDto>> GetAllTerrainsAsync(int adminId, int? siteId = null)
    {
        IEnumerable<Terrain> terrains = await terrainRepository.GetByAdminIdAsync(adminId, siteId);
        return terrains.Select(ToDto);
    }

    // No ownership scoping — same reasoning as SiteService.GetAllSitesPublicAsync().
    public async Task<IEnumerable<TerrainDto>> GetTerrainsBySiteAsync(int siteId)
    {
        IEnumerable<Terrain> terrains = await terrainRepository.GetBySiteIdAsync(siteId);
        return terrains.Select(ToDto);
    }

    public async Task<TerrainDto> CreateTerrainAsync(int adminId, CreateTerrainDto dto)
    {
        await createValidator.ValidateOrThrowAsync(dto);
        await GetOwnedSiteOrThrowAsync(adminId, dto.SiteId);

        var terrain = new Terrain { Name = dto.Name, SiteId = dto.SiteId };
        await terrainRepository.AddAsync(terrain);
        await unitOfWork.SaveChangesAsync();

        return ToDto(terrain);
    }

    public async Task UpdateTerrainAsync(int adminId, int id, UpdateTerrainDto dto)
    {
        await updateValidator.ValidateOrThrowAsync(dto);

        Terrain terrain = await GetOwnedTerrainOrThrowAsync(adminId, id);
        terrain.Name = dto.Name;

        terrainRepository.Update(terrain);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteTerrainAsync(int adminId, int id)
    {
        Terrain terrain = await GetOwnedTerrainOrThrowAsync(adminId, id);
        terrainRepository.Delete(terrain);
        await unitOfWork.SaveChangesAsync();
    }

    private async Task<Terrain> GetOwnedTerrainOrThrowAsync(int adminId, int id)
    {
        Terrain? terrain = await terrainRepository.GetByIdAsync(id);
        if (terrain is null || terrain.Site is null || terrain.Site.AdminId != adminId)
            throw new TerrainNotFoundException(id);
        return terrain;
    }

    private async Task GetOwnedSiteOrThrowAsync(int adminId, int siteId)
    {
        Site? site = await siteRepository.GetByIdAsync(siteId);
        if (site is null || site.AdminId != adminId)
            throw new SiteNotFoundException(siteId);
    }

    private static TerrainDto ToDto(Terrain terrain) => new(terrain.Id, terrain.Name, terrain.SiteId);
}
