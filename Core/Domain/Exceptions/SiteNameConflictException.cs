namespace Core.Domain.Exceptions;

public sealed class SiteNameConflictException() : ConflictException("You already have a site with this name.")
{
    
}