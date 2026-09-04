namespace Core.Domain.Exceptions;

public sealed class MemberMatriculeConflictException() : ConflictException("Un membre avec ce matricule existe déjà.")
{

}
