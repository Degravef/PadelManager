namespace Core.Domain.Exceptions;

public sealed class HoraireSiteConflictException() : ConflictException("Un horaire existe déjà pour ce site et cette année.")
{

}
