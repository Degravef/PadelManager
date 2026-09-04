namespace Core.Domain.Exceptions;

public sealed class SiteScheduleConflictException() : ConflictException("Un horaire existe déjà pour ce site et cette année.")
{

}
