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

public class SiteServiceTests
{
    private readonly Mock<ISiteRepository> _siteRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<CreateSiteDto>> _createValidator = new();
    private readonly Mock<IValidator<UpdateSiteDto>> _updateValidator = new();
    private readonly SiteService _sut;

    public SiteServiceTests()
    {
        _createValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateSiteDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _updateValidator.Setup(v => v.ValidateAsync(It.IsAny<UpdateSiteDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _sut = new SiteService(_siteRepository.Object, _unitOfWork.Object,
            _createValidator.Object, _updateValidator.Object);
    }

    [Fact]
    public async Task GetSiteByIdAsync_OwnedSite_ReturnsDto()
    {
        _siteRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Site { Id = 1, Name = "Site A", Address = "Addr", AdminId = 10 });

        var result = await _sut.GetSiteByIdAsync(adminId: 10, id: 1);

        Assert.Equal("Site A", result.Name);
    }

    [Fact]
    public async Task GetSiteByIdAsync_SiteOwnedByAnotherAdmin_ThrowsSiteNotFoundException()
    {
        _siteRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Site { Id = 1, Name = "Site A", Address = "Addr", AdminId = 99 });

        await Assert.ThrowsAsync<SiteNotFoundException>(() => _sut.GetSiteByIdAsync(adminId: 10, id: 1));
    }

    [Fact]
    public async Task GetSiteByIdAsync_UnknownId_ThrowsSiteNotFoundException()
    {
        _siteRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Site?)null);

        await Assert.ThrowsAsync<SiteNotFoundException>(() => _sut.GetSiteByIdAsync(adminId: 10, id: 999));
    }

    [Fact]
    public async Task GetAllSitesAsync_ReturnsDtosForAdmin()
    {
        _siteRepository.Setup(r => r.GetByAdminIdAsync(10)).ReturnsAsync(
        [
            new Site { Id = 1, Name = "Site A", Address = "Addr A", AdminId = 10 },
            new Site { Id = 2, Name = "Site B", Address = "Addr B", AdminId = 10 }
        ]);

        Assert.Equal(2, (await _sut.GetAllSitesAsync(10)).Count());
    }

    [Fact]
    public async Task CreateSiteAsync_ValidDto_AddsSiteAndSavesOnce()
    {
        var result = await _sut.CreateSiteAsync(adminId: 10, new CreateSiteDto("Site A", "Addr A"));

        _siteRepository.Verify(r => r.AddAsync(It.Is<Site>(s =>
            s.Name == "Site A" && s.Address == "Addr A" && s.AdminId == 10)), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("Site A", result.Name);
    }

    [Fact]
    public async Task CreateSiteAsync_InvalidDto_ThrowsValidationException_AndNeverSaves()
    {
        var dto = new CreateSiteDto("", "Addr A");
        _createValidator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
                [new ValidationFailure(nameof(CreateSiteDto.Name), "'Name' must not be empty.")]));

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateSiteAsync(adminId: 10, dto));

        _siteRepository.Verify(r => r.AddAsync(It.IsAny<Site>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateSiteAsync_UnitOfWorkThrowsConflict_PropagatesUnchanged()
    {
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SiteNameConflictException());

        await Assert.ThrowsAsync<SiteNameConflictException>(
            () => _sut.CreateSiteAsync(adminId: 10, new CreateSiteDto("Site A", "Addr A")));
    }

    [Fact]
    public async Task UpdateSiteAsync_OwnedSite_UpdatesFieldsAndSavesOnce()
    {
        var site = new Site { Id = 1, Name = "Old", Address = "Old Addr", AdminId = 10 };
        _siteRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(site);

        await _sut.UpdateSiteAsync(adminId: 10, id: 1, new UpdateSiteDto("New", "New Addr"));

        Assert.Equal("New", site.Name);
        Assert.Equal("New Addr", site.Address);
        _siteRepository.Verify(r => r.Update(site), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSiteAsync_SiteOwnedByAnotherAdmin_ThrowsSiteNotFoundException_AndNeverSaves()
    {
        _siteRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Site { Id = 1, Name = "Old", Address = "Old Addr", AdminId = 99 });

        await Assert.ThrowsAsync<SiteNotFoundException>(
            () => _sut.UpdateSiteAsync(adminId: 10, id: 1, new UpdateSiteDto("New", "New Addr")));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteSiteAsync_OwnedSite_DeletesAndSavesOnce()
    {
        var site = new Site { Id = 1, Name = "Site A", Address = "Addr", AdminId = 10 };
        _siteRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(site);

        await _sut.DeleteSiteAsync(adminId: 10, id: 1);

        _siteRepository.Verify(r => r.Delete(site), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSiteAsync_SiteOwnedByAnotherAdmin_ThrowsSiteNotFoundException_AndNeverSaves()
    {
        _siteRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Site { Id = 1, Name = "Site A", Address = "Addr", AdminId = 99 });

        await Assert.ThrowsAsync<SiteNotFoundException>(() => _sut.DeleteSiteAsync(adminId: 10, id: 1));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}