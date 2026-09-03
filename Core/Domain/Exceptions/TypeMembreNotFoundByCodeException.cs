namespace Core.Domain.Exceptions;

// ASSUMPTION: practically unreachable — TypeMembre rows are seeded at migration time (see
// Dal/Configurations/TypeMembreConfiguration.cs) for every code MembreService can resolve from a
// matricule prefix. Kept as a named exception, not an assert, for the same reason as the default
// branch in MembreService.ResolveTypeMembreCode().
public sealed class TypeMembreNotFoundByCodeException(string code)
    : NotFoundException($"Le type de membre '{code}' est introuvable.")
{

}
