namespace Core.Domain.Exceptions;

public sealed class CourtNotFoundException(int id) : NotFoundException($"Le terrain avec l'ID {id} est introuvable.")
{

}
