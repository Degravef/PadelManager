namespace Core.Domain.Exceptions;

// RG-PRV-001 / RG-ETA-001: the match already has its 4 active/paid participants.
public sealed class MatchFullException() : ConflictException("Ce match est déjà complet.")
{

}
