namespace Core.Domain.Exceptions;

public sealed class TerrainNameConflictException() : ConflictException("Ce site possède déjà un terrain portant ce nom.")
{

}
