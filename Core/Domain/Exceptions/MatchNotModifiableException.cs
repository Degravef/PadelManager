namespace Core.Domain.Exceptions;

// RG-ETA-005: a match whose date/end time have passed is considered played and no longer modifiable.
public sealed class MatchNotModifiableException() : ConflictException("Ce match est déjà passé et ne peut plus être modifié.")
{

}
