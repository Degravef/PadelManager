namespace Core.Domain.Exceptions;

public sealed class BalanceDueAlreadyPaidException() : ConflictException("Ce solde a déjà été payé.")
{

}
