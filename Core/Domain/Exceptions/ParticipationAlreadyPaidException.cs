namespace Core.Domain.Exceptions;

/// RG-PAY-008
public sealed class ParticipationAlreadyPaidException() : ConflictException("Cette place a déjà été payée.")
{

}
