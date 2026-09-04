namespace Core.Domain.Exceptions;

/// RG-RES-007 / RG-PEN-002
public sealed class ActivePenaltyException() : ConflictException("Vous êtes actuellement sous le coup d'une pénalité de réservation.")
{

}
