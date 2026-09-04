namespace Core.Domain.Exceptions;

/// RG-ETA-005
public sealed class MatchNotModifiableException() : ConflictException("Ce match est déjà passé et ne peut plus être modifié.")
{

}
