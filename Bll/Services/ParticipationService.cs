using Bll.Extensions;
using Bll.Rules;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using Core.Dtos;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using FluentValidation;
using MatchType = Core.Domain.Enums.MatchType;

namespace Bll.Services;

public class ParticipationService(
    IMatchRepository matchRepository,
    IParticipationRepository participationRepository,
    ICourtRepository courtRepository,
    IMemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IValidator<AddPlayerDto> addPlayerValidator) : IParticipationService
{
    public async Task<IEnumerable<ParticipationDto>> GetParticipantsAsync(int matchId)
    {
        Match match = await GetMatchOrThrowAsync(matchId);
        return match.Participations.Select(ToDto);
    }

    // RG-PRV-001/002: only the organizer of a private match can register the other 3 players.
    public async Task<ParticipationDto> AddPlayerToPrivateMatchAsync(string organizerMatricule, int matchId, AddPlayerDto dto)
    {
        await addPlayerValidator.ValidateOrThrowAsync(dto);

        Match match = await GetMatchOrThrowAsync(matchId);
        if (match.Type == MatchType.Public)
            throw new PrivateMatchRegistrationForbiddenException();

        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        if (!MatchModifiableRule.IsModifiable(match, now))
            throw new MatchNotModifiableException();

        Member organizer = await GetMemberOrThrowAsync(organizerMatricule);
        if (match.OrganizerId != organizer.Id)
            throw new MatchNotFoundException(matchId); // ownership non-leak, mirrors SiteService.GetOwnedSiteOrThrowAsync

        if (RosterCompleteRule.HasFourActive(match.Participations))
            throw new MatchFullException();

        Member player = await GetMemberOrThrowAsync(dto.Matricule);
        await EnsureScopeAndNoOverlapAsync(player, match, now);

        Participation participation = await CreateParticipationAsync(match, player.Id, ParticipationRole.Player);
        await unitOfWork.SaveChangesAsync();

        return ToDto(participation);
    }

    // RG-PUB-002/003/004: on a public match, each player registers themself — RG-PEN-004 never blocks
    // this action (an active penalty only prevents creating a new reservation).
    public async Task<ParticipationDto> JoinPublicMatchAsync(string matricule, int matchId)
    {
        Match match = await GetMatchOrThrowAsync(matchId);
        if (match.Type != MatchType.Public)
            throw new JoinPrivateMatchForbiddenException();

        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        if (!MatchModifiableRule.IsModifiable(match, now))
            throw new MatchNotModifiableException();

        if (RosterCompleteRule.HasFourActive(match.Participations))
            throw new MatchFullException();

        Member member = await GetMemberOrThrowAsync(matricule);
        await EnsureScopeAndNoOverlapAsync(member, match, now);

        Participation participation = await CreateParticipationAsync(match, member.Id, ParticipationRole.Player);
        await unitOfWork.SaveChangesAsync();

        return ToDto(participation);
    }

    private async Task EnsureScopeAndNoOverlapAsync(Member member, Match match, DateTime now)
    {
        Court? court = await courtRepository.GetByIdAsync(match.CourtId);
        if (court is null || !MemberScopeRule.CanActOnSite(member, court.SiteId))
            throw new SiteNotAuthorizedException();

        IEnumerable<Participation> activeParticipations = await participationRepository.GetActiveByMemberIdAsync(member.Id);
        if (OverlapRule.IsOverlapping(activeParticipations, match.Id, match.Date, match.StartTime, match.EndTime))
            throw new MatchOverlapException();
    }

    private async Task<Participation> CreateParticipationAsync(Match match, int memberId, ParticipationRole role)
    {
        int seatNumber = Enumerable.Range(1, 4).First(n => match.Participations.All(p => p.SeatNumber != n));

        var participation = new Participation
        {
            MatchId = match.Id,
            MemberId = memberId,
            SeatNumber = seatNumber,
            Role = role,
            Status = ParticipationStatus.Reserved,
            AmountDue = match.TotalAmount / 4m,
            RegistrationDate = timeProvider.GetUtcNow().UtcDateTime
        };
        await participationRepository.AddAsync(participation);
        return participation;
    }

    private async Task<Match> GetMatchOrThrowAsync(int id)
    {
        Match? match = await matchRepository.GetByIdAsync(id);
        if (match is null)
            throw new MatchNotFoundException(id);
        return match;
    }

    private async Task<Member> GetMemberOrThrowAsync(string matricule)
    {
        Member? member = await memberRepository.GetByMatriculeAsync(matricule);
        if (member is null)
            throw new MemberNotFoundByMatriculeException(matricule);
        return member;
    }

    private static ParticipationDto ToDto(Participation p) => new(
        p.Id, p.MatchId, p.MemberId, p.SeatNumber, p.Role.ToString(), p.Status.ToString(),
        p.AmountDue, p.RegistrationDate, p.PaymentDate);
}
