namespace Core.Domain.Exceptions;

public abstract class NotFoundException(string message) : BusinessException(message)
{
    
}