using Core.Domain.Entities;
using Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public class MemberTypeRepository(PadelDbContext context) : IMemberTypeRepository
{
    public async Task<MemberType?> GetByCodeAsync(string code)
    {
        return await context.MemberTypes.AsNoTracking().FirstOrDefaultAsync(t => t.Code == code);
    }
}
