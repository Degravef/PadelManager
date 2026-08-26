namespace Core.Domain.Exceptions;

public sealed class SiteNotFoundException(int id) : NotFoundException($"Le site avec l'ID {id} est introuvable.")
{
    
}