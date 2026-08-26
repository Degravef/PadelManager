namespace Core.Domain.Exceptions;

public sealed class CreneauIndisponibleException() : ConflictException("Ce terrain est déjà réservé à cette date et à cette heure.")
{

}
