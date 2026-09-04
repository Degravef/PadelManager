namespace Core.Domain.Exceptions;

/// RG-SITE-005
public sealed class SlotOutsideOpeningHoursException() : BusinessException("Ce créneau ne correspond à aucun horaire disponible pour ce site.")
{

}
