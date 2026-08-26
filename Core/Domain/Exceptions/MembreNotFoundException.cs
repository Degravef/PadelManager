namespace Core.Domain.Exceptions;

public sealed class MembreNotFoundException(int id) : NotFoundException($"Le membre avec l'ID {id} est introuvable.")
{

}
