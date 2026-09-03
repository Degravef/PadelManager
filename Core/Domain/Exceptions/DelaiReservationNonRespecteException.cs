namespace Core.Domain.Exceptions;


public sealed class DelaiReservationNonRespecteException()
    : BusinessException("Cette réservation est en dehors de la fenêtre autorisée pour votre type de membre.")
{

}
