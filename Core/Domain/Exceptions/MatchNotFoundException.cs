namespace Core.Domain.Exceptions;

public sealed class MatchNotFoundException(int id) : NotFoundException($"Le match avec l'ID {id} est introuvable.")
{

}
