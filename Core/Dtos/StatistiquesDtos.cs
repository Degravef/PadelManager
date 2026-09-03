namespace Core.Dtos;

// RG-PAY-009 / CF-RC-004.
public record ChiffreAffairesDto(decimal Montant, DateOnly Debut, DateOnly Fin);
