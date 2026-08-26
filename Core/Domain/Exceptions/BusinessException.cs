namespace Core.Domain.Exceptions;

public abstract class BusinessException(string message) : Exception(message)
{
    
}