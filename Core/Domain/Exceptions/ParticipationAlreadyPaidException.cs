namespace Core.Domain.Exceptions;

// RG-PAY-008: once paid, a participation can't be paid again (and there's no refund).
public sealed class ParticipationAlreadyPaidException() : ConflictException("Cette place a déjà été payée.")
{

}
