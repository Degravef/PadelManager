namespace Core.Domain.Exceptions;

// RG-RES-006 / RG-PAY-006: an outstanding balance blocks new reservations.
public sealed class BalanceDueException() : ConflictException("Vous avez un solde impayé, vous ne pouvez pas créer de nouvelle réservation.")
{

}
