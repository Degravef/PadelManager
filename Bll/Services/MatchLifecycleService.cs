using Bll.Rules;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Dtos;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Bll.Services;




public class MatchLifecycleService(
    IMatchRepository matchRepository,
    IParticipationRepository participationRepository,
    IPenaliteRepository penaliteRepository,
    ISoldeDuRepository soldeDuRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IMatchLifecycleService
{
    public async Task<TraitementQuotidienResultDto> ExecuterTraitementQuotidienAsync(DateOnly? aujourdHui = null)
    {
        DateOnly today = aujourdHui ?? DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        DateOnly demain = today.AddDays(1);
        DateTime maintenant = timeProvider.GetUtcNow().UtcDateTime;

        var matches = (await matchRepository.GetByDateAsync(demain))
            .Where(m => m.Statut != StatutMatch.Cancelled)
            .ToList();

        int basculesEffectif = 0, basculesPaiement = 0, penalitesAppliquees = 0, soldesCrees = 0, matchesCompletes = 0;

        foreach (Match match in matches)
        {
            if (match.TypeMatch == TypeMatch.Private)
            {
                if (!EffectifCompletRule.AQuatreActifs(match.Participations))
                {
                    
                    BasculerPublic(match, maintenant);
                    basculesEffectif++;

                    await penaliteRepository.AddAsync(new Penalite
                    {
                        MembreId = match.OrganisateurId,
                        MatchId = match.Id,
                        Motif = "Effectif incomplet la veille du match (RG-PRV-004/005).",
                        DateDebut = today,
                        DateFin = today.AddDays(7),
                        Active = true
                    });
                    penalitesAppliquees++;
                }
                else
                {
                    
                    var impayes = match.Participations.Where(p => p.Statut == StatutParticipation.Reservee).ToList();
                    if (impayes.Count > 0)
                    {
                        foreach (Participation impaye in impayes)
                        {
                            participationRepository.Delete(impaye);
                            match.Participations.Remove(impaye);
                        }

                        BasculerPublic(match, maintenant);
                        basculesPaiement++;
                    }
                }
            }

            if (match.TypeMatch == TypeMatch.Public)
            {
                if (EffectifCompletRule.EstComplet(match.Participations))
                {
                    match.Statut = StatutMatch.Complete;
                    matchRepository.Update(match);
                    matchesCompletes++;
                }
                else
                {
                    
                    SoldeDu? existant = await soldeDuRepository.GetByMatchIdAsync(match.Id);
                    if (existant is null)
                    {
                        decimal montant = SoldeOrganisateurRule.CalculerSolde(match.MontantTotal, match.Participations);
                        if (montant > 0)
                        {
                            await soldeDuRepository.AddAsync(new SoldeDu
                            {
                                MembreId = match.OrganisateurId,
                                MatchId = match.Id,
                                Montant = montant,
                                Statut = StatutSoldeDu.Du,
                                DateCreation = maintenant
                            });
                            soldesCrees++;
                        }
                    }
                }
            }
        }

        await unitOfWork.SaveChangesAsync();

        return new TraitementQuotidienResultDto(demain, basculesEffectif, basculesPaiement, penalitesAppliquees, soldesCrees, matchesCompletes);
    }

    private void BasculerPublic(Match match, DateTime maintenant)
    {
        match.TypeMatch = TypeMatch.Public; 
        match.DateBasculePublic = maintenant;
        matchRepository.Update(match);
    }
}
