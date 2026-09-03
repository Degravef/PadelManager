namespace Core.Domain.Exceptions;


public sealed class CreneauHorsHorairesException() : BusinessException("Ce créneau ne correspond à aucun horaire disponible pour ce site.")
{

}
