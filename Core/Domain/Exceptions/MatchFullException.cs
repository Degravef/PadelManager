namespace Core.Domain.Exceptions;

/// RG-PRV-001 / RG-ETA-001
public sealed class MatchFullException() : ConflictException("Ce match est déjà complet.")
{

}
