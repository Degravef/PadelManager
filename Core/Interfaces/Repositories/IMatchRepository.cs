using Core.Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(int id);
    Task<IEnumerable<Match>> GetByTerrainAndDateAsync(int terrainId, DateOnly date);
    Task<IEnumerable<Match>> GetByOrganisateurIdAsync(int organisateurId);
    Task AddAsync(Match match);
}
