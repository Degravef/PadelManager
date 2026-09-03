namespace Core.Domain.Exceptions;

public sealed class SoldeDuNotFoundException(int id) : NotFoundException($"Le solde dû avec l'ID {id} est introuvable.")
{

}
