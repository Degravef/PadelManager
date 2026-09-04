namespace Core.Domain.Exceptions;

/// RG-MEM-006
public sealed class SiteNotAuthorizedException() : BusinessException("Vous n'êtes pas autorisé à réserver sur ce site.")
{

}
