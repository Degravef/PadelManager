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

public class CourtServiceTests
{
    private readonly Mock<ICourtRepository> _courtRepository = new();
    private readonly Mock<ISiteRepository> _siteRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<CreateCourtDto>> _createValidator = new();
    private readonly Mock<IValidator<UpdateCourtDto>> _updateValidator = new();
    private readonly CourtService _sut;

    public CourtServiceTests()
    {
        _createValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateCourtDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _updateValidator.Setup(v => v.ValidateAsync(It.IsAny<UpdateCourtDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _sut = new CourtService(_courtRepository.Object, _siteRepository.Object, _unitOfWork.Object,
            _createValidator.Object, _updateValidator.Object);
    }

    private static Court MakeCourt(int id, string name, int siteId, int siteAdminId) =>
        new() { Id = id, Name = name, SiteId = siteId, Site = new Site { Id = siteId, Name = "S", Address = "A", AdminId = siteAdminId } };

    [Fact]
    public async Task GetCourtByIdAsync_OwnedCourt_ReturnsDto()
    {
        _courtRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeCourt(1, "Court 1", siteId: 5, siteAdminId: 10));

        var result = await _sut.GetCourtByIdAsync(adminId: 10, id: 1);

        Assert.Equal("Court 1", result.Name);
        Assert.Equal(5, result.SiteId);
    }

    [Fact]
    public async Task GetCourtByIdAsync_CourtOnSiteOwnedByAnotherAdmin_ThrowsCourtNotFoundException()
    {
        _courtRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeCourt(1, "Court 1", siteId: 5, siteAdminId: 99));

        await Assert.ThrowsAsync<CourtNotFoundException>(() => _sut.GetCourtByIdAsync(adminId: 10, id: 1));
    }

    [Fact]
    public async Task GetCourtByIdAsync_UnknownId_ThrowsCourtNotFoundException()
    {
        _courtRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Court?)null);

        await Assert.ThrowsAsync<CourtNotFoundException>(() => _sut.GetCourtByIdAsync(adminId: 10, id: 999));
    }

    [Fact]
    public async Task GetAllCourtsAsync_ReturnsDtosForAdmin()
    {
        _courtRepository.Setup(r => r.GetByAdminIdAsync(10, null)).ReturnsAsync(
        [
            MakeCourt(1, "Court 1", siteId: 5, siteAdminId: 10),
            MakeCourt(2, "Court 2", siteId: 6, siteAdminId: 10)
        ]);

        Assert.Equal(2, (await _sut.GetAllCourtsAsync(10)).Count());
    }

    [Fact]
    public async Task GetAllCourtsAsync_WithSiteIdFilter_PassesFilterToRepository()
    {
        _courtRepository.Setup(r => r.GetByAdminIdAsync(10, 5)).ReturnsAsync(
            [MakeCourt(1, "Court 1", siteId: 5, siteAdminId: 10)]);

        var result = (await _sut.GetAllCourtsAsync(10, siteId: 5)).ToList();

        Assert.Single(result);
        _courtRepository.Verify(r => r.GetByAdminIdAsync(10, 5), Times.Once);
    }

    [Fact]
    public async Task GetCourtsBySiteAsync_ReturnsDtosRegardlessOfAdmin()
    {
        _courtRepository.Setup(r => r.GetBySiteIdAsync(5)).ReturnsAsync(
        [
            MakeCourt(1, "Court 1", siteId: 5, siteAdminId: 10),
            MakeCourt(2, "Court 2", siteId: 5, siteAdminId: 10)
        ]);

        var result = (await _sut.GetCourtsBySiteAsync(5)).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task CreateCourtAsync_OwnedSite_AddsCourtAndSavesOnce()
    {
        _siteRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Site { Id = 5, Name = "S", Address = "A", AdminId = 10 });

        var result = await _sut.CreateCourtAsync(adminId: 10, new CreateCourtDto("Court 1", 5));

        _courtRepository.Verify(r => r.AddAsync(It.Is<Court>(t =>
            t.Name == "Court 1" && t.SiteId == 5)), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("Court 1", result.Name);
    }

    [Fact]
    public async Task CreateCourtAsync_SiteOwnedByAnotherAdmin_ThrowsSiteNotFoundException_AndNeverSaves()
    {
        _siteRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Site { Id = 5, Name = "S", Address = "A", AdminId = 99 });

        await Assert.ThrowsAsync<SiteNotFoundException>(
            () => _sut.CreateCourtAsync(adminId: 10, new CreateCourtDto("Court 1", 5)));

        _courtRepository.Verify(r => r.AddAsync(It.IsAny<Court>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateCourtAsync_InvalidDto_ThrowsValidationException_AndNeverSaves()
    {
        var dto = new CreateCourtDto("", 5);
        _createValidator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
                [new ValidationFailure(nameof(CreateCourtDto.Name), "'Name' must not be empty.")]));

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateCourtAsync(adminId: 10, dto));

        _siteRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _courtRepository.Verify(r => r.AddAsync(It.IsAny<Court>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateCourtAsync_UnitOfWorkThrowsConflict_PropagatesUnchanged()
    {
        _siteRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Site { Id = 5, Name = "S", Address = "A", AdminId = 10 });
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CourtNameConflictException());

        await Assert.ThrowsAsync<CourtNameConflictException>(
            () => _sut.CreateCourtAsync(adminId: 10, new CreateCourtDto("Court 1", 5)));
    }

    [Fact]
    public async Task UpdateCourtAsync_OwnedCourt_UpdatesFieldsAndSavesOnce()
    {
        var court = MakeCourt(1, "Old", siteId: 5, siteAdminId: 10);
        _courtRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(court);

        await _sut.UpdateCourtAsync(adminId: 10, id: 1, new UpdateCourtDto("New"));

        Assert.Equal("New", court.Name);
        _courtRepository.Verify(r => r.Update(court), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCourtAsync_CourtOnSiteOwnedByAnotherAdmin_ThrowsCourtNotFoundException_AndNeverSaves()
    {
        _courtRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeCourt(1, "Old", siteId: 5, siteAdminId: 99));

        await Assert.ThrowsAsync<CourtNotFoundException>(
            () => _sut.UpdateCourtAsync(adminId: 10, id: 1, new UpdateCourtDto("New")));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCourtAsync_OwnedCourt_DeletesAndSavesOnce()
    {
        var court = MakeCourt(1, "Court 1", siteId: 5, siteAdminId: 10);
        _courtRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(court);

        await _sut.DeleteCourtAsync(adminId: 10, id: 1);

        _courtRepository.Verify(r => r.Delete(court), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCourtAsync_CourtOnSiteOwnedByAnotherAdmin_ThrowsCourtNotFoundException_AndNeverSaves()
    {
        _courtRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeCourt(1, "Court 1", siteId: 5, siteAdminId: 99));

        await Assert.ThrowsAsync<CourtNotFoundException>(() => _sut.DeleteCourtAsync(adminId: 10, id: 1));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
