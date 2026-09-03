namespace Core.Domain.Exceptions;


public sealed class ChevauchementMatchException() : ConflictException("Vous êtes déjà inscrit à un autre match sur ce créneau horaire.")
{

}
