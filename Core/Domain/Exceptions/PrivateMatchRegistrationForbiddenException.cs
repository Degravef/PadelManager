namespace Core.Domain.Exceptions;

/// RG-PUB-004
public sealed class PrivateMatchRegistrationForbiddenException()
    : BusinessException("Ce match est public : chaque joueur doit s'inscrire lui-même.")
{

}
