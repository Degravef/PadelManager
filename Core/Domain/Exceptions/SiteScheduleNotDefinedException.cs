namespace Core.Domain.Exceptions;

/// RG-SITE-002
public sealed class SiteScheduleNotDefinedException(int siteId, int year)
    : NotFoundException($"Aucun horaire n'est défini pour le site {siteId} pour l'année {year}.")
{

}
