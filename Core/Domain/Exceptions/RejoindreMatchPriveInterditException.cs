namespace Core.Domain.Exceptions;


public sealed class RejoindreMatchPriveInterditException()
    : BusinessException("Ce match est privé : seul l'organisateur peut y inscrire des joueurs.")
{

}
