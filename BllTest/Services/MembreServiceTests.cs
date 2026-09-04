using Bll.Services;
using Core.Constants;
using Core.Domain.Entities;
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
    private readonly Mock<ITypeMembreRepository> _typeMembreRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<CreateMembreDto>> _createValidator = new();
    private readonly MembreService _sut;

    private static TypeMembre Global => new()
    {
        Id = TypeMembreSeed.GlobalId, Code = TypeMembreSeed.GlobalCode, Libelle = "Membre global",
        PrefixeMatricule = "G", DelaiReservationJours = 21
    };

    private static TypeMembre Libre => new()
    {
        Id = TypeMembreSeed.LibreId, Code = TypeMembreSeed.LibreCode, Libelle = "Membre libre",
        PrefixeMatricule = "L", DelaiReservationJours = 5
    };

    private static TypeMembre Site => new()
    {
        Id = TypeMembreSeed.SiteId, Code = TypeMembreSeed.SiteCode, Libelle = "Membre de site",
        PrefixeMatricule = "S", DelaiReservationJours = 14
    };

    public MembreServiceTests()
    {
        _createValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateMembreDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _typeMembreRepository.Setup(r => r.GetByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((string code) => code switch
            {
                TypeMembreSeed.SiteCode => Site,
                TypeMembreSeed.LibreCode => Libre,
                _ => Global
            });

        _sut = new MembreService(_membreRepository.Object, _siteRepository.Object,
            _typeMembreRepository.Object, _unitOfWork.Object, _createValidator.Object, TimeProvider.System);
    }

    [Fact]
    public async Task GetMembreByIdAsync_ExistingMembre_ReturnsDto()
    {
        _membreRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(
            new Membre { Id = 1, Matricule = "G1", Name = "Doe", FirstName = "Jane", TypeMembreId = Global.Id, TypeMembre = Global });

        var result = await _sut.GetMembreByIdAsync(1);

        Assert.Equal("Doe", result.Name);
        Assert.Equal(TypeMembreSeed.GlobalCode, result.TypeMembre);
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
            new Membre { Id = 1, Matricule = "G1", Name = "Doe", FirstName = "Jane", TypeMembreId = Global.Id, TypeMembre = Global });

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
            new Membre { Id = 1, Matricule = "G1", Name = "Doe", FirstName = "Jane", TypeMembreId = Global.Id, TypeMembre = Global },
            new Membre { Id = 2, Matricule = "L1", Name = "Roe", FirstName = "Jim", TypeMembreId = Libre.Id, TypeMembre = Libre }
        ]);

        var result = await _sut.GetAllMembresAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllMatriculesAsync_ReturnsMatriculesFromRepository()
    {
        _membreRepository.Setup(r => r.GetAllMatriculesAsync()).ReturnsAsync(["G1", "L1"]);

        var result = await _sut.GetAllMatriculesAsync();

        Assert.Equal(["G1", "L1"], result);
    }

    [Theory]
    [InlineData(TypeMembreSeed.GlobalCode, TypeMembreSeed.GlobalId, "G")]
    [InlineData(TypeMembreSeed.LibreCode, TypeMembreSeed.LibreId, "L")]
    public async Task CreateMembreAsync_GlobalOrLibre_GeneratesFirstMatriculeAndSavesOnce(
        string typeCode, int expectedTypeId, string prefix)
    {
        _membreRepository.Setup(r => r.GetMatriculesByPrefixAsync(prefix)).ReturnsAsync([]);

        var result = await _sut.CreateMembreAsync(new CreateMembreDto("Doe", "Jane", typeCode, null));

        _membreRepository.Verify(r => r.AddAsync(It.Is<Membre>(m =>
            m.Matricule == $"{prefix}1" && m.TypeMembreId == expectedTypeId && m.SiteId == null)), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal($"{prefix}1", result.Matricule);
        Assert.Equal(typeCode, result.TypeMembre);
    }

    [Fact]
    public async Task CreateMembreAsync_ExistingMatriculesForPrefix_GeneratesNextNumber()
    {
        _membreRepository.Setup(r => r.GetMatriculesByPrefixAsync("G")).ReturnsAsync(["G1", "G3", "G2"]);

        var result = await _sut.CreateMembreAsync(new CreateMembreDto("Doe", "Jane", TypeMembreSeed.GlobalCode, null));

        Assert.Equal("G4", result.Matricule);
    }

    [Fact]
    public async Task CreateMembreAsync_SiteTypeWithExistingSite_SavesOnce()
    {
        _siteRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Site { Id = 5, Name = "A", Address = "B", AdminId = 1 });
        _membreRepository.Setup(r => r.GetMatriculesByPrefixAsync("S")).ReturnsAsync([]);

        var result = await _sut.CreateMembreAsync(new CreateMembreDto("Doe", "Jane", TypeMembreSeed.SiteCode, 5));

        Assert.Equal(5, result.SiteId);
        Assert.Equal("S1", result.Matricule);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateMembreAsync_SiteTypeWithoutSiteId_ThrowsMembreSiteIdInvalideException_AndNeverSaves()
    {
        await Assert.ThrowsAsync<MembreSiteIdInvalideException>(
            () => _sut.CreateMembreAsync(new CreateMembreDto("Doe", "Jane", TypeMembreSeed.SiteCode, null)));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateMembreAsync_GlobalTypeWithSiteId_ThrowsMembreSiteIdInvalideException_AndNeverSaves()
    {
        await Assert.ThrowsAsync<MembreSiteIdInvalideException>(
            () => _sut.CreateMembreAsync(new CreateMembreDto("Doe", "Jane", TypeMembreSeed.GlobalCode, 5)));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateMembreAsync_SiteTypeWithUnknownSiteId_ThrowsSiteNotFoundException_AndNeverSaves()
    {
        _siteRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Site?)null);

        await Assert.ThrowsAsync<SiteNotFoundException>(
            () => _sut.CreateMembreAsync(new CreateMembreDto("Doe", "Jane", TypeMembreSeed.SiteCode, 999)));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateMembreAsync_InvalidDto_ThrowsValidationException_AndNeverSaves()
    {
        var dto = new CreateMembreDto("", "Jane", TypeMembreSeed.GlobalCode, null);
        _createValidator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
                [new ValidationFailure(nameof(CreateMembreDto.Name), "'Name' must not be empty.")]));

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateMembreAsync(dto));

        _membreRepository.Verify(r => r.AddAsync(It.IsAny<Membre>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateMembreAsync_UnitOfWorkThrowsConflict_PropagatesUnchanged()
    {
        _membreRepository.Setup(r => r.GetMatriculesByPrefixAsync("G")).ReturnsAsync([]);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MembreMatriculeConflictException());

        await Assert.ThrowsAsync<MembreMatriculeConflictException>(
            () => _sut.CreateMembreAsync(new CreateMembreDto("Doe", "Jane", TypeMembreSeed.GlobalCode, null)));
    }
}
