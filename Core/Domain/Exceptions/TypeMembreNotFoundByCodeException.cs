namespace Core.Domain.Exceptions;




public sealed class TypeMembreNotFoundByCodeException(string code)
    : NotFoundException($"Le type de membre '{code}' est introuvable.")
{

}
