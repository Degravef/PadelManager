namespace Core.Domain.Exceptions;

// ASSUMPTION: practically unreachable — TypeMembre rows are seeded at migration time (see
// Dal/Configurations/TypeMembreConfiguration.cs) for every code CreateMembreDtoValidator accepts
// for CreateMembreDto.Type. Kept as a named exception rather than an assert regardless.
public sealed class TypeMembreNotFoundByCodeException(string code)
    : NotFoundException($"Le type de membre '{code}' est introuvable.")
{

}
