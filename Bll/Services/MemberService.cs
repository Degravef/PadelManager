using Bll.Extensions;
using Core.Constants;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using Core.Dtos;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using FluentValidation;

namespace Bll.Services;

public class MemberService(
    IMemberRepository memberRepository,
    ISiteRepository siteRepository,
    IMemberTypeRepository memberTypeRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreateMemberDto> createValidator,
    TimeProvider timeProvider) : IMemberService
{
    public async Task<MemberDto> GetMemberByIdAsync(int id)
    {
        Member member = await GetMemberOrThrowAsync(id);
        return ToDto(member);
    }

    public async Task<MemberDto> GetMemberByMatriculeAsync(string matricule)
    {
        Member? member = await memberRepository.GetByMatriculeAsync(matricule);
        if (member is null)
            throw new MemberNotFoundByMatriculeException(matricule);
        return ToDto(member);
    }

    public async Task<IEnumerable<MemberDto>> GetAllMembersAsync()
    {
        IEnumerable<Member> members = await memberRepository.GetAllAsync();
        return members.Select(ToDto);
    }

    public async Task<IEnumerable<string>> GetAllMatriculesAsync()
    {
        return await memberRepository.GetAllMatriculesAsync();
    }

    public async Task<MemberDto> CreateMemberAsync(CreateMemberDto dto)
    {
        await createValidator.ValidateOrThrowAsync(dto);

        bool siteIdProvidedConsistently = (dto.Type == MemberTypeSeed.SiteCode) == (dto.SiteId is not null);
        if (!siteIdProvidedConsistently)
            throw new MemberSiteIdInvalidException();
        if (dto.SiteId is not null)
            await GetSiteOrThrowAsync(dto.SiteId.Value);

        MemberType memberType = await GetMemberTypeOrThrowAsync(dto.Type);
        string matricule = await GenerateMatriculeAsync(memberType);

        var member = new Member
        {
            Matricule = matricule,
            Name = dto.Name,
            FirstName = dto.FirstName,
            MemberTypeId = memberType.Id,
            SiteId = dto.SiteId,
            Email = dto.Email,
            Phone = dto.Phone,
            RegistrationDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime)
        };

        await memberRepository.AddAsync(member);
        await unitOfWork.SaveChangesAsync();

        // BUG FIX: don't set member.MemberType = memberType above — memberType comes from an
        // AsNoTracking() query, so attaching it as a navigation on a newly-Added Member made EF's
        // graph-walk mark that already-seeded MemberType row as Added too, causing a duplicate-PK
        // conflict on every member creation. Passing the code straight through avoids the navigation
        // entirely for this DTO.
        return ToDto(member, memberType.Code);
    }

    // Next free number for the type's prefix (e.g. G1, G2, ...) — relies on the unique-matricule DB
    // constraint (translated to MemberMatriculeConflictException) as the actual safety net against a
    // concurrent registration racing for the same number, per AGENTS.md's "trust the DB constraint"
    // convention, rather than a pre-check-then-insert loop.
    private async Task<string> GenerateMatriculeAsync(MemberType memberType)
    {
        IEnumerable<string> existingMatricules = await memberRepository.GetMatriculesByPrefixAsync(memberType.MatriculePrefix);
        int nextNumber = existingMatricules
            .Select(m => int.TryParse(m[memberType.MatriculePrefix.Length..], out int number) ? number : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;
        return $"{memberType.MatriculePrefix}{nextNumber}";
    }

    private async Task<Member> GetMemberOrThrowAsync(int id)
    {
        Member? member = await memberRepository.GetByIdAsync(id);
        if (member is null)
            throw new MemberNotFoundException(id);
        return member;
    }

    private async Task GetSiteOrThrowAsync(int siteId)
    {
        Site? site = await siteRepository.GetByIdAsync(siteId);
        if (site is null)
            throw new SiteNotFoundException(siteId);
    }

    private async Task<MemberType> GetMemberTypeOrThrowAsync(string code)
    {
        MemberType? memberType = await memberTypeRepository.GetByCodeAsync(code);
        if (memberType is null)
            throw new MemberTypeNotFoundByCodeException(code);
        return memberType;
    }

    private static MemberDto ToDto(Member member) => ToDto(member, member.MemberType!.Code);

    private static MemberDto ToDto(Member member, string memberTypeCode) => new(
        member.Id, member.Matricule, member.Name, member.FirstName,
        memberTypeCode,
        member.SiteId,
        member.BalancesDue.Where(s => s.Status == BalanceDueStatus.Due).Sum(s => s.Amount),
        member.Penalties.Where(p => p.Active).Select(p => (DateTime?)p.EndDate.ToDateTime(TimeOnly.MinValue)).Max(),
        member.Email,
        member.Phone,
        member.Role.ToString(),
        member.RegistrationDate,
        member.Active);
}
