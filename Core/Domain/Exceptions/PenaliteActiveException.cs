namespace Core.Domain.Exceptions;


public sealed class PenaliteActiveException() : ConflictException("Vous êtes actuellement sous le coup d'une pénalité de réservation.")
{

}
