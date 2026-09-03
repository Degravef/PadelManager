namespace Core.Domain.Exceptions;


public sealed class SoldeDuException() : ConflictException("Vous avez un solde impayé, vous ne pouvez pas créer de nouvelle réservation.")
{

}
