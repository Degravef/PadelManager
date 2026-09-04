namespace Core.Domain.Exceptions;

public sealed class CourtNameConflictException() : ConflictException("Ce site possède déjà un terrain portant ce nom.")
{

}
