namespace Core.Domain.Entities;


public class TypeMembre
{
    public int Id { get; set; }
    public required string Code { get; set; } 
    public required string Libelle { get; set; }
    public required string PrefixeMatricule { get; set; } 
    public required int DelaiReservationJours { get; set; } 
    public ICollection<Membre> Membres { get; set; } = [];
}
