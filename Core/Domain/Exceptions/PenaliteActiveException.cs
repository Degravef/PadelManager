namespace Core.Domain.Exceptions;

// RG-RES-007 / RG-PEN-002: an active penalty blocks new reservations.
public sealed class PenaliteActiveException() : ConflictException("Vous êtes actuellement sous le coup d'une pénalité de réservation.")
{

}
