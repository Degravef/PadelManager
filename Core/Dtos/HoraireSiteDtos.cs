namespace Core.Dtos;

public record HoraireSiteDto(
    int Id,
    int SiteId,
    int Annee,
    TimeOnly HeurePremiereReservation,
    TimeOnly HeureDerniereReservation,
    int DureeMatchMinutes,
    int PauseMinutes,
    decimal PrixMatch);

public record CreateHoraireSiteDto(
    int Annee,
    TimeOnly HeurePremiereReservation,
    TimeOnly HeureDerniereReservation,
    decimal? PrixMatch = null);
