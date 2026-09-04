namespace Core.Domain.Exceptions;

// RG-SITE-005: the requested start time doesn't match any generated slot for the site's hours.
public sealed class SlotOutsideOpeningHoursException() : BusinessException("Ce créneau ne correspond à aucun horaire disponible pour ce site.")
{

}
