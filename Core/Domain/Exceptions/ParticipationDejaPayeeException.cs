namespace Core.Domain.Exceptions;


public sealed class ParticipationDejaPayeeException() : ConflictException("Cette place a déjà été payée.")
{

}
