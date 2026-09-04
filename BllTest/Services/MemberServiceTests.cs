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

public class MemberServiceTests
{
    private readonly Mock<IMemberRepository> _memberRepository = new();
    private readonly Mock<ISiteRepository> _siteRepository = new();
    private readonly Mock<IMemberTypeRepository> _memberTypeRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<CreateMemberDto>> _createValidator = new();
    private readonly MemberService _sut;

    private static MemberType Global => new()
    {
        Id = MemberTypeSeed.GlobalId, Code = MemberTypeSeed.GlobalCode, Label = "Membre global",
        MatriculePrefix = "G", ReservationWindowDays = 21
    };

    private static MemberType Libre => new()
    {
        Id = MemberTypeSeed.LibreId, Code = MemberTypeSeed.LibreCode, Label = "Membre libre",
        MatriculePrefix = "L", ReservationWindowDays = 5
    };

    private static MemberType Site => new()
    {
        Id = MemberTypeSeed.SiteId, Code = MemberTypeSeed.SiteCode, Label = "Membre de site",
        MatriculePrefix = "S", ReservationWindowDays = 14
    };

    public MemberServiceTests()
    {
        _createValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateMemberDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _memberTypeRepository.Setup(r => r.GetByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((string code) => code switch
            {
                MemberTypeSeed.SiteCode => Site,
                MemberTypeSeed.LibreCode => Libre,
                _ => Global
            });

        _sut = new MemberService(_memberRepository.Object, _siteRepository.Object,
            _memberTypeRepository.Object, _unitOfWork.Object, _createValidator.Object, TimeProvider.System);
    }

    [Fact]
    public async Task GetMemberByIdAsync_ExistingMember_ReturnsDto()
    {
        _memberRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(
            new Member { Id = 1, Matricule = "G1", Name = "Doe", FirstName = "Jane", MemberTypeId = Global.Id, MemberType = Global });

        var result = await _sut.GetMemberByIdAsync(1);

        Assert.Equal("Doe", result.Name);
        Assert.Equal(MemberTypeSeed.GlobalCode, result.MemberType);
    }

    [Fact]
    public async Task GetMemberByIdAsync_UnknownId_ThrowsMemberNotFoundException()
    {
        _memberRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Member?)null);

        await Assert.ThrowsAsync<MemberNotFoundException>(() => _sut.GetMemberByIdAsync(999));
    }

    [Fact]
    public async Task GetMemberByMatriculeAsync_ExistingMember_ReturnsDto()
    {
        _memberRepository.Setup(r => r.GetByMatriculeAsync("G1")).ReturnsAsync(
            new Member { Id = 1, Matricule = "G1", Name = "Doe", FirstName = "Jane", MemberTypeId = Global.Id, MemberType = Global });

        var result = await _sut.GetMemberByMatriculeAsync("G1");

        Assert.Equal("Doe", result.Name);
    }

    [Fact]
    public async Task GetMemberByMatriculeAsync_UnknownMatricule_ThrowsMemberNotFoundByMatriculeException()
    {
        _memberRepository.Setup(r => r.GetByMatriculeAsync(It.IsAny<string>())).ReturnsAsync((Member?)null);

        await Assert.ThrowsAsync<MemberNotFoundByMatriculeException>(() => _sut.GetMemberByMatriculeAsync("G999"));
    }

    [Fact]
    public async Task GetAllMembersAsync_ReturnsAllAsDtos()
    {
        _memberRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(
        [
            new Member { Id = 1, Matricule = "G1", Name = "Doe", FirstName = "Jane", MemberTypeId = Global.Id, MemberType = Global },
            new Member { Id = 2, Matricule = "L1", Name = "Roe", FirstName = "Jim", MemberTypeId = Libre.Id, MemberType = Libre }
        ]);

        var result = await _sut.GetAllMembersAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllMatriculesAsync_ReturnsMatriculesFromRepository()
    {
        _memberRepository.Setup(r => r.GetAllMatriculesAsync()).ReturnsAsync(["G1", "L1"]);

        var result = await _sut.GetAllMatriculesAsync();

        Assert.Equal(["G1", "L1"], result);
    }

    [Theory]
    [InlineData(MemberTypeSeed.GlobalCode, MemberTypeSeed.GlobalId, "G")]
    [InlineData(MemberTypeSeed.LibreCode, MemberTypeSeed.LibreId, "L")]
    public async Task CreateMemberAsync_GlobalOrLibre_GeneratesFirstMatriculeAndSavesOnce(
        string typeCode, int expectedTypeId, string prefix)
    {
        _memberRepository.Setup(r => r.GetMatriculesByPrefixAsync(prefix)).ReturnsAsync([]);

        var result = await _sut.CreateMemberAsync(new CreateMemberDto("Doe", "Jane", typeCode, null));

        _memberRepository.Verify(r => r.AddAsync(It.Is<Member>(m =>
            m.Matricule == $"{prefix}1" && m.MemberTypeId == expectedTypeId && m.SiteId == null)), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal($"{prefix}1", result.Matricule);
        Assert.Equal(typeCode, result.MemberType);
    }

    [Fact]
    public async Task CreateMemberAsync_ExistingMatriculesForPrefix_GeneratesNextNumber()
    {
        _memberRepository.Setup(r => r.GetMatriculesByPrefixAsync("G")).ReturnsAsync(["G1", "G3", "G2"]);

        var result = await _sut.CreateMemberAsync(new CreateMemberDto("Doe", "Jane", MemberTypeSeed.GlobalCode, null));

        Assert.Equal("G4", result.Matricule);
    }

    [Fact]
    public async Task CreateMemberAsync_SiteTypeWithExistingSite_SavesOnce()
    {
        _siteRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Site { Id = 5, Name = "A", Address = "B", AdminId = 1 });
        _memberRepository.Setup(r => r.GetMatriculesByPrefixAsync("S")).ReturnsAsync([]);

        var result = await _sut.CreateMemberAsync(new CreateMemberDto("Doe", "Jane", MemberTypeSeed.SiteCode, 5));

        Assert.Equal(5, result.SiteId);
        Assert.Equal("S1", result.Matricule);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateMemberAsync_SiteTypeWithoutSiteId_ThrowsMemberSiteIdInvalidException_AndNeverSaves()
    {
        await Assert.ThrowsAsync<MemberSiteIdInvalidException>(
            () => _sut.CreateMemberAsync(new CreateMemberDto("Doe", "Jane", MemberTypeSeed.SiteCode, null)));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateMemberAsync_GlobalTypeWithSiteId_ThrowsMemberSiteIdInvalidException_AndNeverSaves()
    {
        await Assert.ThrowsAsync<MemberSiteIdInvalidException>(
            () => _sut.CreateMemberAsync(new CreateMemberDto("Doe", "Jane", MemberTypeSeed.GlobalCode, 5)));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateMemberAsync_SiteTypeWithUnknownSiteId_ThrowsSiteNotFoundException_AndNeverSaves()
    {
        _siteRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Site?)null);

        await Assert.ThrowsAsync<SiteNotFoundException>(
            () => _sut.CreateMemberAsync(new CreateMemberDto("Doe", "Jane", MemberTypeSeed.SiteCode, 999)));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateMemberAsync_InvalidDto_ThrowsValidationException_AndNeverSaves()
    {
        var dto = new CreateMemberDto("", "Jane", MemberTypeSeed.GlobalCode, null);
        _createValidator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
                [new ValidationFailure(nameof(CreateMemberDto.Name), "'Name' must not be empty.")]));

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateMemberAsync(dto));

        _memberRepository.Verify(r => r.AddAsync(It.IsAny<Member>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateMemberAsync_UnitOfWorkThrowsConflict_PropagatesUnchanged()
    {
        _memberRepository.Setup(r => r.GetMatriculesByPrefixAsync("G")).ReturnsAsync([]);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MemberMatriculeConflictException());

        await Assert.ThrowsAsync<MemberMatriculeConflictException>(
            () => _sut.CreateMemberAsync(new CreateMemberDto("Doe", "Jane", MemberTypeSeed.GlobalCode, null)));
    }
}
