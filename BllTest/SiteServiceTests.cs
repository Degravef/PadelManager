using Bll.Services;
using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Core.Dtos;
using Core.Interfaces.Repositories;
using FluentValidation;
using Moq;

namespace BllTest;

public class SiteServiceTests
{
    private readonly Mock<ISiteRepository> _siteRepositoryMock = new();
    private readonly Mock<IValidator<CreateSiteDto>> _createValidatorMock = new();
    private readonly Mock<IValidator<UpdateSiteDto>> _updateValidatorMock = new();
    private readonly SiteService _siteService;

    public SiteServiceTests()
    {
        _siteService = new SiteService(
            _siteRepositoryMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object);
    }

    [Fact]
    public async Task CreateSiteAsync_HappyPath_ReturnsSiteDto()
    {
        // Arrange
        var dto = new CreateSiteDto("Site 1", "Address 1");
        _siteRepositoryMock.Setup(r => r.ExistsByNameAsync(dto.Name)).ReturnsAsync(false);

        // Act
        var result = await _siteService.CreateSiteAsync(dto);

        // Assert
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.Address, result.Address);
        _siteRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Site>()), Times.Once);
        _siteRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateSiteAsync_DuplicateName_ThrowsConflictException()
    {
        // Arrange
        var dto = new CreateSiteDto("Existing Site", "Address 1");
        _siteRepositoryMock.Setup(r => r.ExistsByNameAsync(dto.Name)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _siteService.CreateSiteAsync(dto));
    }

    [Fact]
    public async Task GetSiteByIdAsync_ExistingId_ReturnsSiteDto()
    {
        // Arrange
        var site = new Site { Id = 1, Name = "Site 1", Address = "Address 1" };
        _siteRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(site);

        // Act
        var result = await _siteService.GetSiteByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(site.Id, result.Id);
        Assert.Equal(site.Name, result.Name);
    }

    [Fact]
    public async Task GetSiteByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        _siteRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Site?)null);

        // Act
        var result = await _siteService.GetSiteByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateSiteAsync_NonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        _siteRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Site?)null);
        var dto = new UpdateSiteDto("New Name", "New Address");

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _siteService.UpdateSiteAsync(1, dto));
    }
}