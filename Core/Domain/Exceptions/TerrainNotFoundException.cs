namespace Core.Domain.Exceptions;

public sealed class TerrainNotFoundException(int id) : NotFoundException($"Le terrain avec l'ID {id} est introuvable.")
{

}
