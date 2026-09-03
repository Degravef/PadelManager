namespace Core.Domain.Exceptions;

// RG-ETA-006: a member can't hold two seats on matches happening at the same time.
public sealed class ChevauchementMatchException() : ConflictException("Vous êtes déjà inscrit à un autre match sur ce créneau horaire.")
{

}
