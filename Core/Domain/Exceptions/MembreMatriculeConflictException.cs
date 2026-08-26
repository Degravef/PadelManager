namespace Core.Domain.Exceptions;

public sealed class MembreMatriculeConflictException() : ConflictException("Un membre avec ce matricule existe déjà.")
{

}
