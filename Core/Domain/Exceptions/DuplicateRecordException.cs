namespace Core.Domain.Exceptions;

public sealed class DuplicateRecordException() : ConflictException("A record with the same unique value already exists.")
{
    
}