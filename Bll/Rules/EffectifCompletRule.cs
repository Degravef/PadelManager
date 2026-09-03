using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Bll.Rules;

/// <summary>RG-PRV-001 / RG-ETA-001: participant headcount helpers for a match.</summary>
public static class EffectifCompletRule
{
    public static int NombreActifs(IEnumerable<Participation> participations) =>
        participations.Count(p => p.Statut is StatutParticipation.Reservee or StatutParticipation.Payee);

    public static int NombrePayes(IEnumerable<Participation> participations) =>
        participations.Count(p => p.Statut == StatutParticipation.Payee);

    public static bool AQuatreActifs(IEnumerable<Participation> participations) => NombreActifs(participations) >= 4;

    public static bool EstComplet(IEnumerable<Participation> participations) => NombrePayes(participations) >= 4;
}
