using Core.Dtos;

namespace Core.Interfaces.Services;

public interface IMembreService
{
    Task<MembreDto> GetMembreByIdAsync(int id);
    Task<MembreDto> GetMembreByMatriculeAsync(string matricule);
    Task<IEnumerable<MembreDto>> GetAllMembresAsync();
    Task<IEnumerable<string>> GetAllMatriculesAsync();
    Task<MembreDto> CreateMembreAsync(string matricule, CreateMembreDto dto);
}
