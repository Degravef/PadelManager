using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace Web.Services.Context;

public class UserContext
{
    public string Id { get; set; } = "1";
    public Role Role { get; set; } = Role.Admin;
    
    public UserContext(NavigationManager navigationManager)
    {
        var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);

        if (query.TryGetValue("role", out var roleStr) && Enum.TryParse<Role>(roleStr, out var role))
            Role = role;

        if (query.TryGetValue("id", out var idStr))
            Id = idStr!;
    }
}