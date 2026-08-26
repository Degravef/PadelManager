using Core.Dtos;

namespace Core.Interfaces.Services;

public interface IMembreService
{
    Task<MembreDto> GetMembreByIdAsync(int id);
    Task<MembreDto> CreateMembreAsync(string matricule, CreateMembreDto dto);
}
