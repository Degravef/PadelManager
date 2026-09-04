namespace Core.Domain.Exceptions;

/// RG-ETA-006
public sealed class MatchOverlapException() : ConflictException("Vous êtes déjà inscrit à un autre match sur ce créneau horaire.")
{

}
