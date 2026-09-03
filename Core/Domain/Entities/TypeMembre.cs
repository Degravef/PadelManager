namespace Core.Domain.Entities;

// Reference/rule table (readme.md ERD): booking-window lead time per member type is data, not code.
public class TypeMembre
{
    public int Id { get; set; }
    public required string Code { get; set; } // GLOBAL / SITE / LIBRE
    public required string Libelle { get; set; }
    public required string PrefixeMatricule { get; set; } // G / S / L
    public required int DelaiReservationJours { get; set; } // 21 / 14 / 5
    public ICollection<Membre> Membres { get; set; } = [];
}
