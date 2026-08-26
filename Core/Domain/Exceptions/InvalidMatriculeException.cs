namespace Core.Domain.Exceptions;

public sealed class InvalidMatriculeException() : BusinessException("Le matricule fourni est invalide.")
{

}
