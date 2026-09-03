namespace Core.Domain.Exceptions;

public sealed class SoldeDuDejaPayeException() : ConflictException("Ce solde a déjà été payé.")
{

}
