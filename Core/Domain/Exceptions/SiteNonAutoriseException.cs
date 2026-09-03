namespace Core.Domain.Exceptions;

// RG-MEM-006: a Site-type member is scoped to their own site.
public sealed class SiteNonAutoriseException() : BusinessException("Vous n'êtes pas autorisé à réserver sur ce site.")
{

}
