namespace Core.Domain.Exceptions;


public sealed class JourFermeException() : ConflictException("Le site est fermé à cette date.")
{

}
