namespace Core.Domain.Exceptions;

/// RG-PRV-002
public sealed class JoinPrivateMatchForbiddenException()
    : BusinessException("Ce match est privé : seul l'organisateur peut y inscrire des joueurs.")
{

}
