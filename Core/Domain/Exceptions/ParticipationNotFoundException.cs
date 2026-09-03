namespace Core.Domain.Exceptions;

public sealed class ParticipationNotFoundException(int id) : NotFoundException($"La participation avec l'ID {id} est introuvable.")
{

}
