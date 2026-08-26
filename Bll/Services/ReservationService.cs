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

namespace Bll.Services;

public class ReservationService(
    IMatchRepository matchRepository,
    IParticipationRepository participationRepository,
    ITerrainRepository terrainRepository,
    IMembreRepository membreRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IValidator<CreerReservationDto> createValidator) : IReservationService
{
    public async Task<MatchDto> GetReservationByIdAsync(int id)
    {
        Match match = await GetMatchOrThrowAsync(id);
        return ToDto(match);
    }

    public async Task<IEnumerable<MatchDto>> GetMyReservationsAsync(string matricule)
    {
        Membre membre = await GetMembreOrThrowAsync(matricule);
        IEnumerable<Match> matches = await matchRepository.GetByOrganisateurIdAsync(membre.Id);
        return matches.Select(ToDto);
    }

    // ASSUMPTION: happy-path only (BACKLOG.md Tier 1) — creates a Private match with the calling
    // member as organizer and sole participant. Real slot availability (Site_Horaire, Jour_Fermeture,
    // the 15-minute buffer — CF-RC-005) isn't implemented; this only rejects an exact double-booking
    // on the same court/date/start time (see CreneauDisponibleRule). The per-type booking-window rule
    // (CF-RV-001/002/003) and the outstanding-balance block (CF-RV-016) are deferred to a later pass.
    public async Task<MatchDto> CreerReservationAsync(string matricule, CreerReservationDto dto)
    {
        await createValidator.ValidateOrThrowAsync(dto);

        Membre organisateur = await GetMembreOrThrowAsync(matricule);
        await GetTerrainOrThrowAsync(dto.TerrainId);

        IEnumerable<Match> matchsMemeJourMemeTerrain = await matchRepository.GetByTerrainAndDateAsync(dto.TerrainId, dto.Date);
        if (!CreneauDisponibleRule.EstDisponible(matchsMemeJourMemeTerrain, dto.StartTime))
            throw new CreneauIndisponibleException();

        var match = new Match
        {
            TerrainId = dto.TerrainId,
            Date = dto.Date,
            StartTime = dto.StartTime,
            TypeMatch = TypeMatch.Private,
            Statut = StatutMatch.Open,
            OrganisateurId = organisateur.Id,
            MontantTotal = 60m
        };
        await matchRepository.AddAsync(match);

        var participation = new Participation
        {
            Match = match,
            MembreId = organisateur.Id,
            MontantDu = 15m,
            DateInscription = timeProvider.GetUtcNow().UtcDateTime
        };
        await participationRepository.AddAsync(participation);

        await unitOfWork.SaveChangesAsync();

        return ToDto(match);
    }

    private async Task<Match> GetMatchOrThrowAsync(int id)
    {
        Match? match = await matchRepository.GetByIdAsync(id);
        if (match is null)
            throw new MatchNotFoundException(id);
        return match;
    }

    private async Task<Membre> GetMembreOrThrowAsync(string matricule)
    {
        Membre? membre = await membreRepository.GetByMatriculeAsync(matricule);
        if (membre is null)
            throw new MembreNotFoundByMatriculeException(matricule);
        return membre;
    }

    private async Task GetTerrainOrThrowAsync(int terrainId)
    {
        Terrain? terrain = await terrainRepository.GetByIdAsync(terrainId);
        if (terrain is null)
            throw new TerrainNotFoundException(terrainId);
    }

    private static MatchDto ToDto(Match match) => new(
        match.Id, match.TerrainId, match.Date, match.StartTime,
        match.TypeMatch.ToString(), match.Statut.ToString(), match.OrganisateurId, match.MontantTotal);
}
