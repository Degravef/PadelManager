namespace Core.Domain.Exceptions;

public sealed class MembreNotFoundByMatriculeException(string matricule)
    : NotFoundException($"Aucun membre trouvé avec le matricule {matricule}.")
{

}
