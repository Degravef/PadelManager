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
    IHoraireSiteRepository horaireSiteRepository,
    IJourFermetureRepository jourFermetureRepository,
    ISoldeDuRepository soldeDuRepository,
    IPenaliteRepository penaliteRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IValidator<CreerReservationDto> createValidator) : IReservationService
{
    public async Task<MatchDto> GetReservationByIdAsync(string matricule, int id)
    {
        Match match = await GetMatchOrThrowAsync(id);

        
        if (match.TypeMatch == TypeMatch.Private)
        {
            Membre? caller = await membreRepository.GetByMatriculeAsync(matricule);
            bool estParticipant = caller is not null && match.Participations.Any(p => p.MembreId == caller.Id);
            if (!estParticipant)
                throw new MatchNotFoundException(id);
        }

        return ToDto(match);
    }

    public async Task<IEnumerable<MatchDto>> GetMyReservationsAsync(string matricule)
    {
        Membre membre = await GetMembreOrThrowAsync(matricule);
        IEnumerable<Match> matches = await matchRepository.GetByOrganisateurIdAsync(membre.Id);
        return matches.Select(ToDto);
    }

    public async Task<MatchDto> CreerReservationAsync(string matricule, CreerReservationDto dto)
    {
        await createValidator.ValidateOrThrowAsync(dto);

        Membre organisateur = await GetMembreOrThrowAsync(matricule);
        Terrain terrain = await GetTerrainOrThrowAsync(dto.TerrainId);

        
        if (!terrain.Actif)
            throw new TerrainInactifException();

        
        if (!PorteeMembreRule.PeutAgirSurSite(organisateur, terrain.SiteId))
            throw new SiteNonAutoriseException();

        
        DateOnly aujourdHui = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        if (!DelaiReservationRule.EstDansLaFenetre(organisateur.TypeMembre!.DelaiReservationJours, dto.Date, aujourdHui))
            throw new DelaiReservationNonRespecteException();

        
        IEnumerable<SoldeDu> soldesImpaye = await soldeDuRepository.GetOutstandingByMembreIdAsync(organisateur.Id);
        if (SoldeDuRule.ADuSoldeImpaye(soldesImpaye))
            throw new SoldeDuException();

        
        IEnumerable<Penalite> penalitesActives = await penaliteRepository.GetActiveByMembreIdAsync(organisateur.Id);
        if (PenaliteActiveRule.EstActive(penalitesActives, aujourdHui))
            throw new PenaliteActiveException();

        
        HoraireSite? horaire = await horaireSiteRepository.GetBySiteAndYearAsync(terrain.SiteId, dto.Date.Year);
        if (horaire is null)
            throw new HorairesSiteNonDefinisException(terrain.SiteId, dto.Date.Year);
        if (!CreneauxDisponiblesRule.Calculer(horaire).Contains(dto.StartTime))
            throw new CreneauHorsHorairesException();

        
        IEnumerable<JourFermeture> fermeturesSite = await jourFermetureRepository.GetBySiteIdAsync(terrain.SiteId);
        IEnumerable<JourFermeture> fermeturesGlobales = await jourFermetureRepository.GetGlobalAsync();
        if (!JourOuvertRule.EstOuvert(fermeturesSite.Concat(fermeturesGlobales), dto.Date))
            throw new JourFermeException();

        
        IEnumerable<Match> matchsMemeJourMemeTerrain = await matchRepository.GetByTerrainAndDateAsync(dto.TerrainId, dto.Date);
        if (!CreneauDisponibleRule.EstDisponible(matchsMemeJourMemeTerrain, dto.StartTime))
            throw new CreneauIndisponibleException();

        
        TimeOnly heureFin = dto.StartTime.Add(TimeSpan.FromMinutes(horaire.DureeMatchMinutes));
        IEnumerable<Participation> participationsActives = await participationRepository.GetActiveByMembreIdAsync(organisateur.Id);
        if (ChevauchementRule.EstEnChevauchement(participationsActives, matchIdActuel: 0, dto.Date, dto.StartTime, heureFin))
            throw new ChevauchementMatchException();

        var match = new Match
        {
            TerrainId = dto.TerrainId,
            Date = dto.Date,
            StartTime = dto.StartTime,
            EndTime = heureFin,
            TypeMatch = dto.EstPublic ? TypeMatch.Public : TypeMatch.Private,
            Statut = StatutMatch.Open,
            OrganisateurId = organisateur.Id,
            MontantTotal = horaire.PrixMatch,
            DateCreation = timeProvider.GetUtcNow().UtcDateTime,
            DateLimite = dto.Date.AddDays(-1)
        };
        await matchRepository.AddAsync(match);

        var participation = new Participation
        {
            Match = match,
            MatchId = match.Id, 
            MembreId = organisateur.Id,
            NumeroPlace = 1,
            Role = RoleParticipation.Organisateur,
            Statut = StatutParticipation.Reservee,
            MontantDu = horaire.PrixMatch / 4m,
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

    private async Task<Terrain> GetTerrainOrThrowAsync(int terrainId)
    {
        Terrain? terrain = await terrainRepository.GetByIdAsync(terrainId);
        if (terrain is null)
            throw new TerrainNotFoundException(terrainId);
        return terrain;
    }

    private static MatchDto ToDto(Match match) => new(
        match.Id, match.TerrainId, match.Date, match.StartTime,
        match.TypeMatch.ToString(), match.Statut.ToString(), match.OrganisateurId, match.MontantTotal);
}
