namespace Core.Domain.Exceptions;

// RG-SITE-002: no opening hours defined for this site/year yet.
public sealed class HorairesSiteNonDefinisException(int siteId, int annee)
    : NotFoundException($"Aucun horaire n'est défini pour le site {siteId} pour l'année {annee}.")
{

}
