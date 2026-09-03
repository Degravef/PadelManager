namespace Core.Domain.Exceptions;


public sealed class MatchCompletException() : ConflictException("Ce match est déjà complet.")
{

}
