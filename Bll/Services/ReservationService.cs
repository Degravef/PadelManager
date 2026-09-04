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

        // RG-PRV-003: a private match is only visible to its own registered participants.
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

        // RG-RES-002: le terrain doit être actif.
        if (!terrain.Actif)
            throw new TerrainInactifException();

        // RG-MEM-005/006/007: un membre de site est limité à son propre site.
        if (!PorteeMembreRule.PeutAgirSurSite(organisateur, terrain.SiteId))
            throw new SiteNonAutoriseException();

        // RG-RES-001: fenêtre de réservation propre au type de membre.
        DateOnly aujourdHui = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        if (!DelaiReservationRule.EstDansLaFenetre(organisateur.TypeMembre!.DelaiReservationJours, dto.Date, aujourdHui))
            throw new DelaiReservationNonRespecteException();

        // RG-RES-006 / RG-PAY-006: pas de nouvelle réservation tant qu'un solde reste dû.
        IEnumerable<SoldeDu> soldesImpaye = await soldeDuRepository.GetOutstandingByMembreIdAsync(organisateur.Id);
        if (SoldeDuRule.ADuSoldeImpaye(soldesImpaye))
            throw new SoldeDuException();

        // RG-RES-007 / RG-PEN-002: pas de nouvelle réservation pendant une pénalité active.
        IEnumerable<Penalite> penalitesActives = await penaliteRepository.GetActiveByMembreIdAsync(organisateur.Id);
        if (PenaliteActiveRule.EstActive(penalitesActives, aujourdHui))
            throw new PenaliteActiveException();

        // RG-SITE-002/003/004/005: le créneau doit correspondre aux horaires définis pour le site et l'année.
        HoraireSite? horaire = await horaireSiteRepository.GetBySiteAndYearAsync(terrain.SiteId, dto.Date.Year);
        if (horaire is null)
            throw new HorairesSiteNonDefinisException(terrain.SiteId, dto.Date.Year);
        if (!CreneauxDisponiblesRule.Calculer(horaire).Contains(dto.StartTime))
            throw new CreneauHorsHorairesException();

        // RG-SITE-007/008: pas de réservation un jour de fermeture (site ou global).
        IEnumerable<JourFermeture> fermeturesSite = await jourFermetureRepository.GetBySiteIdAsync(terrain.SiteId);
        IEnumerable<JourFermeture> fermeturesGlobales = await jourFermetureRepository.GetGlobalAsync();
        if (!JourOuvertRule.EstOuvert(fermeturesSite.Concat(fermeturesGlobales), dto.Date))
            throw new JourFermeException();

        // RG-SITE-006: un seul match par terrain et par créneau.
        IEnumerable<Match> matchsMemeJourMemeTerrain = await matchRepository.GetByTerrainAndDateAsync(dto.TerrainId, dto.Date);
        if (!CreneauDisponibleRule.EstDisponible(matchsMemeJourMemeTerrain, dto.StartTime))
            throw new CreneauIndisponibleException();

        // RG-ETA-006: un membre ne peut pas occuper deux places sur des matches simultanés.
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
            MatchId = match.Id, // fixed up by EF from the Match nav once match.Id is assigned at SaveChanges
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

    public async Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsAsync(int siteId, DateOnly date)
    {
        // RG-SITE-002/003/004/005: no hours defined for that year means no bookable slot.
        HoraireSite? horaire = await horaireSiteRepository.GetBySiteAndYearAsync(siteId, date.Year);
        if (horaire is null)
            return [];

        // RG-SITE-007/008: closed days (site-specific or global) have no bookable slot.
        IEnumerable<JourFermeture> fermeturesSite = await jourFermetureRepository.GetBySiteIdAsync(siteId);
        IEnumerable<JourFermeture> fermeturesGlobales = await jourFermetureRepository.GetGlobalAsync();
        if (!JourOuvertRule.EstOuvert(fermeturesSite.Concat(fermeturesGlobales), date))
            return [];

        IReadOnlyList<TimeOnly> heuresDebutPossibles = CreneauxDisponiblesRule.Calculer(horaire);
        TimeSpan dureeMatch = TimeSpan.FromMinutes(horaire.DureeMatchMinutes);

        // RG-RES-002: only active courts can be booked.
        IEnumerable<Terrain> terrains = (await terrainRepository.GetBySiteIdAsync(siteId)).Where(t => t.Actif);
        IEnumerable<Match> matchsDuJour = (await matchRepository.GetBySiteAndDateAsync(siteId, date)).ToList();

        var slots = new List<AvailableSlotDto>();
        foreach (Terrain terrain in terrains)
        {
            IEnumerable<Match> matchsMemeTerrain = matchsDuJour.Where(m => m.TerrainId == terrain.Id);
            foreach (TimeOnly heureDebut in heuresDebutPossibles)
            {
                // RG-SITE-006: one match per court and slot.
                if (CreneauDisponibleRule.EstDisponible(matchsMemeTerrain, heureDebut))
                    slots.Add(new AvailableSlotDto(terrain.Id, terrain.Name, heureDebut, heureDebut.Add(dureeMatch)));
            }
        }

        return slots.OrderBy(s => s.StartTime).ThenBy(s => s.TerrainName);
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
