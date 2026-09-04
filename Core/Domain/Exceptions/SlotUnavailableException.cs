namespace Core.Domain.Exceptions;

public sealed class SlotUnavailableException() : ConflictException("Ce terrain est déjà réservé à cette date et à cette heure.")
{

}
