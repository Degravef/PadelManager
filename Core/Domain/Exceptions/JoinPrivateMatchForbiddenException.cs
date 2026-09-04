namespace Core.Domain.Exceptions;

// RG-PRV-002: a private match can't be self-joined; only the organizer registers its other players.
public sealed class JoinPrivateMatchForbiddenException()
    : BusinessException("Ce match est privé : seul l'organisateur peut y inscrire des joueurs.")
{

}
