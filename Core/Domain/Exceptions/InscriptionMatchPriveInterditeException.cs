namespace Core.Domain.Exceptions;


public sealed class InscriptionMatchPriveInterditeException()
    : BusinessException("Ce match est public : chaque joueur doit s'inscrire lui-même.")
{

}
