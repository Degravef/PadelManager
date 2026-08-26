using Bll.Services;
using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Core.Dtos;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace BllTest.Services;

public class TerrainServiceTests
{
    private readonly Mock<ITerrainRepository> _terrainRepository = new();
    private readonly Mock<ISiteRepository> _siteRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<CreateTerrainDto>> _createValidator = new();
    private readonly Mock<IValidator<UpdateTerrainDto>> _updateValidator = new();
    private readonly TerrainService _sut;

    public TerrainServiceTests()
    {
        _createValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateTerrainDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _updateValidator.Setup(v => v.ValidateAsync(It.IsAny<UpdateTerrainDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _sut = new TerrainService(_terrainRepository.Object, _siteRepository.Object, _unitOfWork.Object,
            _createValidator.Object, _updateValidator.Object);
    }

    private static Terrain MakeTerrain(int id, string name, int siteId, int siteAdminId) =>
        new() { Id = id, Name = name, SiteId = siteId, Site = new Site { Id = siteId, Name = "S", Address = "A", AdminId = siteAdminId } };

    [Fact]
    public async Task GetTerrainByIdAsync_OwnedTerrain_ReturnsDto()
    {
        _terrainRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeTerrain(1, "Court 1", siteId: 5, siteAdminId: 10));

        var result = await _sut.GetTerrainByIdAsync(adminId: 10, id: 1);

        Assert.Equal("Court 1", result.Name);
        Assert.Equal(5, result.SiteId);
    }

    [Fact]
    public async Task GetTerrainByIdAsync_TerrainOnSiteOwnedByAnotherAdmin_ThrowsTerrainNotFoundException()
    {
        _terrainRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeTerrain(1, "Court 1", siteId: 5, siteAdminId: 99));

        await Assert.ThrowsAsync<TerrainNotFoundException>(() => _sut.GetTerrainByIdAsync(adminId: 10, id: 1));
    }

    [Fact]
    public async Task GetTerrainByIdAsync_UnknownId_ThrowsTerrainNotFoundException()
    {
        _terrainRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Terrain?)null);

        await Assert.ThrowsAsync<TerrainNotFoundException>(() => _sut.GetTerrainByIdAsync(adminId: 10, id: 999));
    }

    [Fact]
    public async Task GetAllTerrainsAsync_ReturnsDtosForAdmin()
    {
        _terrainRepository.Setup(r => r.GetByAdminIdAsync(10, null)).ReturnsAsync(
        [
            MakeTerrain(1, "Court 1", siteId: 5, siteAdminId: 10),
            MakeTerrain(2, "Court 2", siteId: 6, siteAdminId: 10)
        ]);

        Assert.Equal(2, (await _sut.GetAllTerrainsAsync(10)).Count());
    }

    [Fact]
    public async Task GetAllTerrainsAsync_WithSiteIdFilter_PassesFilterToRepository()
    {
        _terrainRepository.Setup(r => r.GetByAdminIdAsync(10, 5)).ReturnsAsync(
            [MakeTerrain(1, "Court 1", siteId: 5, siteAdminId: 10)]);

        var result = (await _sut.GetAllTerrainsAsync(10, siteId: 5)).ToList();

        Assert.Single(result);
        _terrainRepository.Verify(r => r.GetByAdminIdAsync(10, 5), Times.Once);
    }

    [Fact]
    public async Task CreateTerrainAsync_OwnedSite_AddsTerrainAndSavesOnce()
    {
        _siteRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Site { Id = 5, Name = "S", Address = "A", AdminId = 10 });

        var result = await _sut.CreateTerrainAsync(adminId: 10, new CreateTerrainDto("Court 1", 5));

        _terrainRepository.Verify(r => r.AddAsync(It.Is<Terrain>(t =>
            t.Name == "Court 1" && t.SiteId == 5)), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("Court 1", result.Name);
    }

    [Fact]
    public async Task CreateTerrainAsync_SiteOwnedByAnotherAdmin_ThrowsSiteNotFoundException_AndNeverSaves()
    {
        _siteRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Site { Id = 5, Name = "S", Address = "A", AdminId = 99 });

        await Assert.ThrowsAsync<SiteNotFoundException>(
            () => _sut.CreateTerrainAsync(adminId: 10, new CreateTerrainDto("Court 1", 5)));

        _terrainRepository.Verify(r => r.AddAsync(It.IsAny<Terrain>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateTerrainAsync_InvalidDto_ThrowsValidationException_AndNeverSaves()
    {
        var dto = new CreateTerrainDto("", 5);
        _createValidator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
                [new ValidationFailure(nameof(CreateTerrainDto.Name), "'Name' must not be empty.")]));

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateTerrainAsync(adminId: 10, dto));

        _siteRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _terrainRepository.Verify(r => r.AddAsync(It.IsAny<Terrain>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateTerrainAsync_UnitOfWorkThrowsConflict_PropagatesUnchanged()
    {
        _siteRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Site { Id = 5, Name = "S", Address = "A", AdminId = 10 });
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TerrainNameConflictException());

        await Assert.ThrowsAsync<TerrainNameConflictException>(
            () => _sut.CreateTerrainAsync(adminId: 10, new CreateTerrainDto("Court 1", 5)));
    }

    [Fact]
    public async Task UpdateTerrainAsync_OwnedTerrain_UpdatesFieldsAndSavesOnce()
    {
        var terrain = MakeTerrain(1, "Old", siteId: 5, siteAdminId: 10);
        _terrainRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(terrain);

        await _sut.UpdateTerrainAsync(adminId: 10, id: 1, new UpdateTerrainDto("New"));

        Assert.Equal("New", terrain.Name);
        _terrainRepository.Verify(r => r.Update(terrain), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTerrainAsync_TerrainOnSiteOwnedByAnotherAdmin_ThrowsTerrainNotFoundException_AndNeverSaves()
    {
        _terrainRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeTerrain(1, "Old", siteId: 5, siteAdminId: 99));

        await Assert.ThrowsAsync<TerrainNotFoundException>(
            () => _sut.UpdateTerrainAsync(adminId: 10, id: 1, new UpdateTerrainDto("New")));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTerrainAsync_OwnedTerrain_DeletesAndSavesOnce()
    {
        var terrain = MakeTerrain(1, "Court 1", siteId: 5, siteAdminId: 10);
        _terrainRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(terrain);

        await _sut.DeleteTerrainAsync(adminId: 10, id: 1);

        _terrainRepository.Verify(r => r.Delete(terrain), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTerrainAsync_TerrainOnSiteOwnedByAnotherAdmin_ThrowsTerrainNotFoundException_AndNeverSaves()
    {
        _terrainRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeTerrain(1, "Court 1", siteId: 5, siteAdminId: 99));

        await Assert.ThrowsAsync<TerrainNotFoundException>(() => _sut.DeleteTerrainAsync(adminId: 10, id: 1));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
