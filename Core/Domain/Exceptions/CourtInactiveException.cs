namespace Core.Domain.Exceptions;

public sealed class CourtInactiveException() : BusinessException("Ce terrain n'est pas actif.")
{

}
