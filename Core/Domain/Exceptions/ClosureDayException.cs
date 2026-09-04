namespace Core.Domain.Exceptions;

/// RG-SITE-007/008
public sealed class ClosureDayException() : ConflictException("Le site est fermé à cette date.")
{

}
