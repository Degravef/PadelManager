using Web.Core.Dtos;

namespace Web.Core.Interfaces;

public interface IMembreService
{
    Task<MembreDto> GetMembreByIdAsync(int id);
    Task<MembreDto> CreateMembreAsync(CreateMembreDto dto);
}
