using Core.Dtos;

namespace Web.Services;

public class IdentityState
{
    public string? CurrentRole { get; set; } // "Membre" or "Administrateur"
    public string? CurrentId { get; set; }
    public MembreDto? CurrentMembre { get; set; }
    public AdministrateurDto? CurrentAdmin { get; set; }

    public event Action? OnChange;

    public void SetMembre(MembreDto membre)
    {
        CurrentRole = "Membre";
        CurrentId = membre.Matricule;
        CurrentMembre = membre;
        CurrentAdmin = null;
        NotifyStateChanged();
    }

    public void SetAdmin(AdministrateurDto admin)
    {
        CurrentRole = "Administrateur";
        CurrentId = admin.Id;
        CurrentAdmin = admin;
        CurrentMembre = null;
        NotifyStateChanged();
    }

    public void Clear()
    {
        CurrentRole = null;
        CurrentId = null;
        CurrentMembre = null;
        CurrentAdmin = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
