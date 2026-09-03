namespace Core.Domain.Exceptions;


public sealed class MatchNonModifiableException() : ConflictException("Ce match est déjà passé et ne peut plus être modifié.")
{

}
