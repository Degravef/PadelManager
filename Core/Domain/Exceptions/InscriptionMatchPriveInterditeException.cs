namespace Core.Domain.Exceptions;

// RG-PUB-004: a public match can't be joined through the organizer-adds-player flow.
public sealed class InscriptionMatchPriveInterditeException()
    : BusinessException("Ce match est public : chaque joueur doit s'inscrire lui-même.")
{

}
