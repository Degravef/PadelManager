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

public class HoraireSiteServiceTests
{
    private readonly Mock<IHoraireSiteRepository> _horaireSiteRepository = new();
    private readonly Mock<ISiteRepository> _siteRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<CreateHoraireSiteDto>> _createValidator = new();
    private readonly HoraireSiteService _sut;

    private static readonly Site OwnedSite = new() { Id = 1, Name = "Site A", Address = "Addr", AdminId = 42 };
    private static readonly CreateHoraireSiteDto ValidDto = new(2026, new TimeOnly(8, 0), new TimeOnly(21, 0));

    public HoraireSiteServiceTests()
    {
        _createValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateHoraireSiteDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _siteRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(OwnedSite);

        _sut = new HoraireSiteService(_horaireSiteRepository.Object, _siteRepository.Object, _unitOfWork.Object, _createValidator.Object);
    }

    [Fact]
    public async Task CreateAsync_HappyPath_AddsHoraireAndSaves()
    {
        var result = await _sut.CreateAsync(42, 1, ValidDto);

        _horaireSiteRepository.Verify(r => r.AddAsync(It.Is<HoraireSite>(h => h.SiteId == 1 && h.Annee == 2026 && h.PrixMatch == 60m)), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(1, result.SiteId);
    }

    [Fact]
    public async Task CreateAsync_CustomPrixMatch_IsUsedInsteadOfDefault()
    {
        var dto = ValidDto with { PrixMatch = 75m };

        var result = await _sut.CreateAsync(42, 1, dto);

        Assert.Equal(75m, result.PrixMatch);
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
    public async Task GetBySiteAndYearAsync_ExistingHoraire_ReturnsDto()
    {
        _horaireSiteRepository.Setup(r => r.GetBySiteAndYearAsync(1, 2026)).ReturnsAsync(
            new HoraireSite { Id = 5, SiteId = 1, Annee = 2026, HeurePremiereReservation = new TimeOnly(8, 0), HeureDerniereReservation = new TimeOnly(21, 0), PrixMatch = 60m });

        var result = await _sut.GetBySiteAndYearAsync(42, 1, 2026);

        Assert.Equal(5, result.Id);
    }

    [Fact]
    public async Task GetBySiteAndYearAsync_NoHoraireForYear_ThrowsHorairesSiteNonDefinisException()
    {
        _horaireSiteRepository.Setup(r => r.GetBySiteAndYearAsync(1, 2027)).ReturnsAsync((HoraireSite?)null);

        await Assert.ThrowsAsync<HorairesSiteNonDefinisException>(() => _sut.GetBySiteAndYearAsync(42, 1, 2027));
    }
}
