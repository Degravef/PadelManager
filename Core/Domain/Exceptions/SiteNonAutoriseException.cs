namespace Core.Domain.Exceptions;


public sealed class SiteNonAutoriseException() : BusinessException("Vous n'êtes pas autorisé à réserver sur ce site.")
{

}
