using Bll.Rules;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using Core.Dtos;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Bll.Services;

public class PaiementService(
    IParticipationRepository participationRepository,
    IMatchRepository matchRepository,
    IMembreRepository membreRepository,
    ISoldeDuRepository soldeDuRepository,
    IPaiementRepository paiementRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IPaiementService
{
    // RG-PAY-003/007/008, RG-ETA-001: paie la place, absorbe un solde impayé éventuel, et complète le
    // match si les 4 places sont désormais payées.
    public async Task<PaiementDto> PayerParticipationAsync(string matricule, int participationId, PayerDto dto)
    {
        Participation participation = await GetParticipationOrThrowAsync(participationId);
        Membre membre = await GetMembreOrThrowAsync(matricule);
        if (participation.MembreId != membre.Id)
            throw new ParticipationNotFoundException(participationId); // ownership non-leak

        Match match = participation.Match!;
        DateTime maintenant = timeProvider.GetUtcNow().UtcDateTime;
        if (!MatchModifiableRule.EstModifiable(match, maintenant))
            throw new MatchNonModifiableException();
        if (participation.Statut == StatutParticipation.Payee)
            throw new ParticipationDejaPayeeException();

        // RG-PAY-007: un solde dû est ajouté au montant de la prochaine inscription payée.
        var soldesImpaye = (await soldeDuRepository.GetOutstandingByMembreIdAsync(membre.Id)).ToList();
        decimal montantSolde = soldesImpaye.Sum(s => s.Montant);

        var paiement = new Paiement
        {
            MembreId = membre.Id,
            ParticipationId = participationId,
            // ASSUMPTION: Paiement->SoldeDu is a single FK; when several soldes are folded in, only the
            // first is linked for traceability while every outstanding solde still gets marked Payé below.
            SoldeDuId = soldesImpaye.Count > 0 ? soldesImpaye[0].Id : null,
            Montant = participation.MontantDu + montantSolde,
            DatePaiement = maintenant,
            MoyenPaiement = dto.MoyenPaiement ?? string.Empty,
            Statut = StatutPaiement.Valide
        };
        await paiementRepository.AddAsync(paiement);

        foreach (SoldeDu solde in soldesImpaye)
        {
            solde.Statut = StatutSoldeDu.Paye;
            solde.DateReglement = maintenant;
            soldeDuRepository.Update(solde);
        }

        participation.Statut = StatutParticipation.Payee;
        participation.DateValidation = maintenant;
        participationRepository.Update(participation);

        // RG-ETA-001: le match est complet une fois ses 4 places payées.
        var autresParticipations = await participationRepository.GetByMatchIdAsync(match.Id);
        int nbPayesApres = EffectifCompletRule.NombrePayes(autresParticipations) + 1; // +1 : cette place, pas encore persistée
        if (nbPayesApres >= 4)
        {
            match.Statut = StatutMatch.Complete;
            matchRepository.Update(match);
        }

        await unitOfWork.SaveChangesAsync();

        return ToDto(paiement);
    }

    // RG-PAY-005/006: permet à un membre redevable de solder sa dette directement, sans attendre de
    // rejoindre un autre match payant.
    public async Task<PaiementDto> PayerSoldeAsync(string matricule, int soldeDuId, PayerDto dto)
    {
        SoldeDu? solde = await soldeDuRepository.GetByIdAsync(soldeDuId);
        if (solde is null)
            throw new SoldeDuNotFoundException(soldeDuId);

        Membre membre = await GetMembreOrThrowAsync(matricule);
        if (solde.MembreId != membre.Id)
            throw new SoldeDuNotFoundException(soldeDuId); // ownership non-leak

        if (solde.Statut == StatutSoldeDu.Paye)
            throw new SoldeDuDejaPayeException();

        DateTime maintenant = timeProvider.GetUtcNow().UtcDateTime;
        var paiement = new Paiement
        {
            MembreId = membre.Id,
            SoldeDuId = soldeDuId,
            Montant = solde.Montant,
            DatePaiement = maintenant,
            MoyenPaiement = dto.MoyenPaiement ?? string.Empty,
            Statut = StatutPaiement.Valide
        };
        await paiementRepository.AddAsync(paiement);

        solde.Statut = StatutSoldeDu.Paye;
        solde.DateReglement = maintenant;
        soldeDuRepository.Update(solde);

        await unitOfWork.SaveChangesAsync();

        return ToDto(paiement);
    }

    // Lets a member discover what they owe (and its id) before calling PayerSoldeAsync — RG-RES-006
    // blocks new reservations while a solde is outstanding, so a member needs a way to find it.
    public async Task<IEnumerable<SoldeDuDto>> GetMesSoldesImpayesAsync(string matricule)
    {
        Membre membre = await GetMembreOrThrowAsync(matricule);
        var soldes = await soldeDuRepository.GetOutstandingByMembreIdAsync(membre.Id);
        return soldes.Select(ToDto);
    }

    private async Task<Participation> GetParticipationOrThrowAsync(int id)
    {
        Participation? participation = await participationRepository.GetByIdAsync(id);
        if (participation is null)
            throw new ParticipationNotFoundException(id);
        return participation;
    }

    private async Task<Membre> GetMembreOrThrowAsync(string matricule)
    {
        Membre? membre = await membreRepository.GetByMatriculeAsync(matricule);
        if (membre is null)
            throw new MembreNotFoundByMatriculeException(matricule);
        return membre;
    }

    private static PaiementDto ToDto(Paiement p) => new(
        p.Id, p.MembreId, p.ParticipationId, p.SoldeDuId, p.Montant, p.DatePaiement, p.Statut.ToString());

    private static SoldeDuDto ToDto(SoldeDu s) => new(s.Id, s.MembreId, s.MatchId, s.Montant, s.Statut.ToString(), s.DateCreation);
}
