namespace Core.Domain.Exceptions;


public sealed class HorairesSiteNonDefinisException(int siteId, int annee)
    : NotFoundException($"Aucun horaire n'est défini pour le site {siteId} pour l'année {annee}.")
{

}
