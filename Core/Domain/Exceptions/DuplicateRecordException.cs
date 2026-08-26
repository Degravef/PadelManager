namespace Core.Domain.Exceptions;

public sealed class DuplicateRecordException() : ConflictException("Un enregistrement avec cette valeur unique existe déjà.")
{
    
}