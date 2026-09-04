namespace Core.Domain.Exceptions;

public sealed class MemberTypeNotFoundByCodeException(string code)
    : NotFoundException($"Le type de membre '{code}' est introuvable.")
{

}
