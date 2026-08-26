namespace Core.Domain.Exceptions;

public sealed class SiteNotFoundException(int id) : NotFoundException($"Site with ID {id} not found.")
{
    
}