using Bll.Extensions;
using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Core.Dtos;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using FluentValidation;

namespace Bll.Services;

public class CourtService(
    ICourtRepository courtRepository,
    ISiteRepository siteRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreateCourtDto> createValidator,
    IValidator<UpdateCourtDto> updateValidator) : ICourtService
{
    public async Task<CourtDto> GetCourtByIdAsync(int adminId, int id)
    {
        Court court = await GetOwnedCourtOrThrowAsync(adminId, id);
        return ToDto(court);
    }

    public async Task<IEnumerable<CourtDto>> GetAllCourtsAsync(int adminId, int? siteId = null)
    {
        IEnumerable<Court> courts = await courtRepository.GetByAdminIdAsync(adminId, siteId);
        return courts.Select(ToDto);
    }

    public async Task<IEnumerable<CourtDto>> GetCourtsBySiteAsync(int siteId)
    {
        IEnumerable<Court> courts = await courtRepository.GetBySiteIdAsync(siteId);
        return courts.Select(ToDto);
    }

    public async Task<CourtDto> CreateCourtAsync(int adminId, CreateCourtDto dto)
    {
        await createValidator.ValidateOrThrowAsync(dto);
        await GetOwnedSiteOrThrowAsync(adminId, dto.SiteId);

        var court = new Court
        {
            Name = dto.Name,
            SiteId = dto.SiteId,
            Number = dto.Number,
            SurfaceType = dto.SurfaceType,
            Covered = dto.Covered
        };
        await courtRepository.AddAsync(court);
        await unitOfWork.SaveChangesAsync();

        return ToDto(court);
    }

    public async Task UpdateCourtAsync(int adminId, int id, UpdateCourtDto dto)
    {
        await updateValidator.ValidateOrThrowAsync(dto);

        Court court = await GetOwnedCourtOrThrowAsync(adminId, id);
        court.Name = dto.Name;
        court.Number = dto.Number;
        court.SurfaceType = dto.SurfaceType;
        court.Covered = dto.Covered;
        court.Active = dto.Active;

        courtRepository.Update(court);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteCourtAsync(int adminId, int id)
    {
        Court court = await GetOwnedCourtOrThrowAsync(adminId, id);
        courtRepository.Delete(court);
        await unitOfWork.SaveChangesAsync();
    }

    private async Task<Court> GetOwnedCourtOrThrowAsync(int adminId, int id)
    {
        Court? court = await courtRepository.GetByIdAsync(id);
        if (court is null || court.Site is null || court.Site.AdminId != adminId)
            throw new CourtNotFoundException(id);
        return court;
    }

    private async Task GetOwnedSiteOrThrowAsync(int adminId, int siteId)
    {
        Site? site = await siteRepository.GetByIdAsync(siteId);
        if (site is null || site.AdminId != adminId)
            throw new SiteNotFoundException(siteId);
    }

    private static CourtDto ToDto(Court court) => new(
        court.Id, court.Name, court.SiteId, court.Number, court.SurfaceType, court.Covered, court.Active);
}
