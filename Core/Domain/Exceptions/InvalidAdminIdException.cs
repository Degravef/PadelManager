namespace Core.Domain.Exceptions;

public sealed class InvalidAdminIdException() : BusinessException("L'identifiant administrateur fourni est invalide.")
{

}
