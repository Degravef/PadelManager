namespace Core.Domain.Exceptions;

public sealed class ParticipationConflictException() : ConflictException("Vous êtes déjà inscrit à ce match.")
{

}
