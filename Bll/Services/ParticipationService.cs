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

public class ParticipationService(
    IMatchRepository matchRepository,
    IParticipationRepository participationRepository,
    ITerrainRepository terrainRepository,
    IMembreRepository membreRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IValidator<AjouterJoueurDto> ajouterJoueurValidator) : IParticipationService
{
    public async Task<IEnumerable<ParticipationDto>> GetParticipantsAsync(int matchId)
    {
        Match match = await GetMatchOrThrowAsync(matchId);
        return match.Participations.Select(ToDto);
    }

    // RG-PRV-001/002: seul l'organisateur d'un match privé y inscrit les 3 autres joueurs.
    public async Task<ParticipationDto> AjouterJoueurMatchPriveAsync(string matriculeOrganisateur, int matchId, AjouterJoueurDto dto)
    {
        await ajouterJoueurValidator.ValidateOrThrowAsync(dto);

        Match match = await GetMatchOrThrowAsync(matchId);
        if (match.TypeMatch == TypeMatch.Public)
            throw new InscriptionMatchPriveInterditeException();

        DateTime maintenant = timeProvider.GetUtcNow().UtcDateTime;
        if (!MatchModifiableRule.EstModifiable(match, maintenant))
            throw new MatchNonModifiableException();

        Membre organisateur = await GetMembreOrThrowAsync(matriculeOrganisateur);
        if (match.OrganisateurId != organisateur.Id)
            throw new MatchNotFoundException(matchId); // ownership non-leak, mirrors SiteService.GetOwnedSiteOrThrowAsync

        if (EffectifCompletRule.AQuatreActifs(match.Participations))
            throw new MatchCompletException();

        Membre joueur = await GetMembreOrThrowAsync(dto.Matricule);
        await EnsureScopeAndNoOverlapAsync(joueur, match, maintenant);

        Participation participation = await CreerParticipationAsync(match, joueur.Id, RoleParticipation.Joueur);
        await unitOfWork.SaveChangesAsync();

        return ToDto(participation);
    }

    // RG-PUB-002/003/004: sur un match public, chaque joueur s'inscrit lui-même — jamais RG-PEN-004 ne
    // bloque cette action (une pénalité active n'empêche que la création d'une nouvelle réservation).
    public async Task<ParticipationDto> RejoindreMatchPublicAsync(string matricule, int matchId)
    {
        Match match = await GetMatchOrThrowAsync(matchId);
        if (match.TypeMatch != TypeMatch.Public)
            throw new RejoindreMatchPriveInterditException();

        DateTime maintenant = timeProvider.GetUtcNow().UtcDateTime;
        if (!MatchModifiableRule.EstModifiable(match, maintenant))
            throw new MatchNonModifiableException();

        if (EffectifCompletRule.AQuatreActifs(match.Participations))
            throw new MatchCompletException();

        Membre membre = await GetMembreOrThrowAsync(matricule);
        await EnsureScopeAndNoOverlapAsync(membre, match, maintenant);

        Participation participation = await CreerParticipationAsync(match, membre.Id, RoleParticipation.Joueur);
        await unitOfWork.SaveChangesAsync();

        return ToDto(participation);
    }

    private async Task EnsureScopeAndNoOverlapAsync(Membre membre, Match match, DateTime maintenant)
    {
        Terrain? terrain = await terrainRepository.GetByIdAsync(match.TerrainId);
        if (terrain is null || !PorteeMembreRule.PeutAgirSurSite(membre, terrain.SiteId))
            throw new SiteNonAutoriseException();

        IEnumerable<Participation> participationsActives = await participationRepository.GetActiveByMembreIdAsync(membre.Id);
        if (ChevauchementRule.EstEnChevauchement(participationsActives, match.Id, match.Date, match.StartTime, match.EndTime))
            throw new ChevauchementMatchException();
    }

    private async Task<Participation> CreerParticipationAsync(Match match, int membreId, RoleParticipation role)
    {
        int numeroPlace = Enumerable.Range(1, 4).First(n => match.Participations.All(p => p.NumeroPlace != n));

        var participation = new Participation
        {
            MatchId = match.Id,
            MembreId = membreId,
            NumeroPlace = numeroPlace,
            Role = role,
            Statut = StatutParticipation.Reservee,
            MontantDu = match.MontantTotal / 4m,
            DateInscription = timeProvider.GetUtcNow().UtcDateTime
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

    private async Task<Membre> GetMembreOrThrowAsync(string matricule)
    {
        Membre? membre = await membreRepository.GetByMatriculeAsync(matricule);
        if (membre is null)
            throw new MembreNotFoundByMatriculeException(matricule);
        return membre;
    }

    private static ParticipationDto ToDto(Participation p) => new(
        p.Id, p.MatchId, p.MembreId, p.NumeroPlace, p.Role.ToString(), p.Statut.ToString(),
        p.MontantDu, p.DateInscription, p.DateValidation);
}
