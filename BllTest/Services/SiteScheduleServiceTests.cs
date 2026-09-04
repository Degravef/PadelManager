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

public class SiteScheduleServiceTests
{
    private readonly Mock<ISiteScheduleRepository> _siteScheduleRepository = new();
    private readonly Mock<ISiteRepository> _siteRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<CreateSiteScheduleDto>> _createValidator = new();
    private readonly SiteScheduleService _sut;

    private static readonly Site OwnedSite = new() { Id = 1, Name = "Site A", Address = "Addr", AdminId = 42 };
    private static readonly CreateSiteScheduleDto ValidDto = new(2026, new TimeOnly(8, 0), new TimeOnly(21, 0));

    public SiteScheduleServiceTests()
    {
        _createValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateSiteScheduleDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _siteRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(OwnedSite);

        _sut = new SiteScheduleService(_siteScheduleRepository.Object, _siteRepository.Object, _unitOfWork.Object, _createValidator.Object);
    }

    [Fact]
    public async Task CreateAsync_HappyPath_AddsScheduleAndSaves()
    {
        var result = await _sut.CreateAsync(42, 1, ValidDto);

        _siteScheduleRepository.Verify(r => r.AddAsync(It.Is<SiteSchedule>(h => h.SiteId == 1 && h.Year == 2026 && h.MatchPrice == 60m)), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(1, result.SiteId);
    }

    [Fact]
    public async Task CreateAsync_CustomMatchPrice_IsUsedInsteadOfDefault()
    {
        var dto = ValidDto with { MatchPrice = 75m };

        var result = await _sut.CreateAsync(42, 1, dto);

        Assert.Equal(75m, result.MatchPrice);
    }

    [Fact]
    public async Task CreateAsync_SiteNotOwnedByAdmin_ThrowsSiteNotFoundException()
    {
        await Assert.ThrowsAsync<SiteNotFoundException>(() => _sut.CreateAsync(999, 1, ValidDto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_UnknownSite_ThrowsSiteNotFoundException()
    {
        _siteRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Site?)null);

        await Assert.ThrowsAsync<SiteNotFoundException>(() => _sut.CreateAsync(42, 999, ValidDto));
    }

    [Fact]
    public async Task GetBySiteAndYearAsync_ExistingSchedule_ReturnsDto()
    {
        _siteScheduleRepository.Setup(r => r.GetBySiteAndYearAsync(1, 2026)).ReturnsAsync(
            new SiteSchedule { Id = 5, SiteId = 1, Year = 2026, OpeningTime = new TimeOnly(8, 0), ClosingTime = new TimeOnly(21, 0), MatchPrice = 60m });

        var result = await _sut.GetBySiteAndYearAsync(42, 1, 2026);

        Assert.Equal(5, result.Id);
    }

    [Fact]
    public async Task GetBySiteAndYearAsync_NoScheduleForYear_ThrowsSiteScheduleNotDefinedException()
    {
        _siteScheduleRepository.Setup(r => r.GetBySiteAndYearAsync(1, 2027)).ReturnsAsync((SiteSchedule?)null);

        await Assert.ThrowsAsync<SiteScheduleNotDefinedException>(() => _sut.GetBySiteAndYearAsync(42, 1, 2027));
    }
}
