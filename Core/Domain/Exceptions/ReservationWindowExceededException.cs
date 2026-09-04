namespace Core.Domain.Exceptions;

/// RG-RES-001
public sealed class ReservationWindowExceededException()
    : BusinessException("Cette réservation est en dehors de la fenêtre autorisée pour votre type de membre.")
{

}
