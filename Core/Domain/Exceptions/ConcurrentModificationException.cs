namespace Core.Domain.Exceptions;

public sealed class ConcurrentModificationException() : ConflictException("Cette ressource a été modifiée entretemps par une autre opération, veuillez réessayer.")
{

}
