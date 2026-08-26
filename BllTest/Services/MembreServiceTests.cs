using Bll.Services;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using Core.Dtos;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace BllTest.Services;

public class MembreServiceTests
{
    private readonly Mock<IMembreRepository> _membreRepository = new();
    private readonly Mock<ISiteRepository> _siteRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<CreateMembreDto>> _createValidator = new();
    private readonly MembreService _sut;

    public MembreServiceTests()
    {
        _createValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateMembreDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _sut = new MembreService(_membreRepository.Object, _siteRepository.Object,
            _unitOfWork.Object, _createValidator.Object);
    }

    [Fact]
    public async Task GetMembreByIdAsync_ExistingMembre_ReturnsDto()
    {
        _membreRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(
            new Membre { Id = 1, Matricule = "G1", Name = "Doe", FirstName = "Jane", TypeMembre = TypeMembre.Global });

        var result = await _sut.GetMembreByIdAsync(1);

        Assert.Equal("Doe", result.Name);
        Assert.Equal("Global", result.TypeMembre);
    }

    [Fact]
    public async Task GetMembreByIdAsync_UnknownId_ThrowsMembreNotFoundException()
    {
        _membreRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Membre?)null);

        await Assert.ThrowsAsync<MembreNotFoundException>(() => _sut.GetMembreByIdAsync(999));
    }

    [Fact]
    public async Task GetMembreByMatriculeAsync_ExistingMembre_ReturnsDto()
    {
        _membreRepository.Setup(r => r.GetByMatriculeAsync("G1")).ReturnsAsync(
            new Membre { Id = 1, Matricule = "G1", Name = "Doe", FirstName = "Jane", TypeMembre = TypeMembre.Global });

        var result = await _sut.GetMembreByMatriculeAsync("G1");

        Assert.Equal("Doe", result.Name);
    }

    [Fact]
    public async Task GetMembreByMatriculeAsync_UnknownMatricule_ThrowsMembreNotFoundByMatriculeException()
    {
        _membreRepository.Setup(r => r.GetByMatriculeAsync(It.IsAny<string>())).ReturnsAsync((Membre?)null);

        await Assert.ThrowsAsync<MembreNotFoundByMatriculeException>(() => _sut.GetMembreByMatriculeAsync("G999"));
    }

    [Fact]
    public async Task GetAllMembresAsync_ReturnsAllAsDtos()
    {
        _membreRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(
        [
            new Membre { Id = 1, Matricule = "G1", Name = "Doe", FirstName = "Jane", TypeMembre = TypeMembre.Global },
            new Membre { Id = 2, Matricule = "L1", Name = "Roe", FirstName = "Jim", TypeMembre = TypeMembre.Libre }
        ]);

        var result = await _sut.GetAllMembresAsync();

        Assert.Equal(2, result.Count());
    }

    [Theory]
    [InlineData("G1", TypeMembre.Global)]
    [InlineData("L1", TypeMembre.Libre)]
    public async Task CreateMembreAsync_GlobalOrLibreMatricule_DerivesTypeAndSavesOnce(string matricule, TypeMembre expectedType)
    {
        var result = await _sut.CreateMembreAsync(matricule, new CreateMembreDto("Doe", "Jane", null));

        _membreRepository.Verify(r => r.AddAsync(It.Is<Membre>(m =>
            m.Matricule == matricule && m.TypeMembre == expectedType && m.SiteId == null)), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(expectedType.ToString(), result.TypeMembre);
    }

    [Fact]
    public async Task CreateMembreAsync_SiteMatriculeWithExistingSite_SavesOnce()
    {
        _siteRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Site { Id = 5, Name = "A", Address = "B", AdminId = 1 });

        var result = await _sut.CreateMembreAsync("S1", new CreateMembreDto("Doe", "Jane", 5));

        Assert.Equal(5, result.SiteId);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateMembreAsync_SiteMatriculeWithoutSiteId_ThrowsMembreSiteIdInvalideException_AndNeverSaves()
    {
        await Assert.ThrowsAsync<MembreSiteIdInvalideException>(
            () => _sut.CreateMembreAsync("S1", new CreateMembreDto("Doe", "Jane", null)));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateMembreAsync_GlobalMatriculeWithSiteId_ThrowsMembreSiteIdInvalideException_AndNeverSaves()
    {
        await Assert.ThrowsAsync<MembreSiteIdInvalideException>(
            () => _sut.CreateMembreAsync("G1", new CreateMembreDto("Doe", "Jane", 5)));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateMembreAsync_SiteMatriculeWithUnknownSiteId_ThrowsSiteNotFoundException_AndNeverSaves()
    {
        _siteRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Site?)null);

        await Assert.ThrowsAsync<SiteNotFoundException>(
            () => _sut.CreateMembreAsync("S1", new CreateMembreDto("Doe", "Jane", 999)));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateMembreAsync_InvalidDto_ThrowsValidationException_AndNeverSaves()
    {
        var dto = new CreateMembreDto("", "Jane", null);
        _createValidator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
                [new ValidationFailure(nameof(CreateMembreDto.Name), "'Name' must not be empty.")]));

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateMembreAsync("G1", dto));

        _membreRepository.Verify(r => r.AddAsync(It.IsAny<Membre>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateMembreAsync_UnitOfWorkThrowsConflict_PropagatesUnchanged()
    {
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MembreMatriculeConflictException());

        await Assert.ThrowsAsync<MembreMatriculeConflictException>(
            () => _sut.CreateMembreAsync("G1", new CreateMembreDto("Doe", "Jane", null)));
    }
}
