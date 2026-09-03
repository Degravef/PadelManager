namespace Core.Domain.Exceptions;

// RG-SITE-007/008: closure day (site-specific or global) blocks reservations.
public sealed class JourFermeException() : ConflictException("Le site est fermé à cette date.")
{

}
