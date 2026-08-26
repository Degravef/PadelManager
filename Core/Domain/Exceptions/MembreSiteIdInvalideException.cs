namespace Core.Domain.Exceptions;

public sealed class MembreSiteIdInvalideException() : BusinessException(
    "Le site est obligatoire pour un membre de type Site, et ne peut pas être renseigné pour les autres types de membre.")
{

}
