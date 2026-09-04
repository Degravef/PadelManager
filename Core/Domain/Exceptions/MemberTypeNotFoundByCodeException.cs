namespace Core.Domain.Exceptions;

// ASSUMPTION: practically unreachable — MemberType rows are seeded at migration time (see
// Dal/Configurations/MemberTypeConfiguration.cs) for every code CreateMemberDtoValidator accepts
// for CreateMemberDto.Type. Kept as a named exception rather than an assert regardless.
public sealed class MemberTypeNotFoundByCodeException(string code)
    : NotFoundException($"Le type de membre '{code}' est introuvable.")
{

}
