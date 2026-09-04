namespace Web.Core.Interfaces;

public interface IIdentityService
{
    Task<IEnumerable<int>> GetAdminIdsAsync();
    Task<IEnumerable<string>> GetMatriculesAsync();
}
