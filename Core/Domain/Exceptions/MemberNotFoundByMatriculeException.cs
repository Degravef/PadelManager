namespace Core.Domain.Exceptions;

public sealed class MemberNotFoundByMatriculeException(string matricule)
    : NotFoundException($"Aucun membre trouvé avec le matricule {matricule}.")
{

}
