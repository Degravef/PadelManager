namespace Core.Domain.Exceptions;

public abstract class ConflictException(string message) : BusinessException(message)
{
    
}