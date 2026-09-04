using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface IMemberTypeRepository
{
    Task<MemberType?> GetByCodeAsync(string code);
}
