namespace Core.Domain.Exceptions;

public sealed class SiteNameConflictException() : ConflictException("Vous possédez déjà un site portant ce nom.")
{
    
}