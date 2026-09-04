namespace Core.Domain.Exceptions;

public sealed class BalanceDueNotFoundException(int id) : NotFoundException($"Le solde dû avec l'ID {id} est introuvable.")
{

}
