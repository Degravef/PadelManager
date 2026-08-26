using Bll.Extensions;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using Core.Dtos;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using FluentValidation;

namespace Bll.Services;

public class MembreService(
    IMembreRepository membreRepository,
    ISiteRepository siteRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreateMembreDto> createValidator) : IMembreService
{
    public async Task<MembreDto> GetMembreByIdAsync(int id)
    {
        Membre membre = await GetMembreOrThrowAsync(id);
        return ToDto(membre);
    }

    public async Task<MembreDto> GetMembreByMatriculeAsync(string matricule)
    {
        Membre? membre = await membreRepository.GetByMatriculeAsync(matricule);
        if (membre is null)
            throw new MembreNotFoundByMatriculeException(matricule);
        return ToDto(membre);
    }

    public async Task<IEnumerable<MembreDto>> GetAllMembresAsync()
    {
        IEnumerable<Membre> membres = await membreRepository.GetAllAsync();
        return membres.Select(ToDto);
    }

    public async Task<MembreDto> CreateMembreAsync(string matricule, CreateMembreDto dto)
    {
        await createValidator.ValidateOrThrowAsync(dto);

        TypeMembre type = ResolveTypeMembre(matricule);
        bool siteIdProvidedConsistently = (type == TypeMembre.Site) == (dto.SiteId is not null);
        if (!siteIdProvidedConsistently)
            throw new MembreSiteIdInvalideException();
        if (dto.SiteId is not null)
            await GetSiteOrThrowAsync(dto.SiteId.Value);

        var membre = new Membre
        {
            Matricule = matricule,
            Name = dto.Name,
            FirstName = dto.FirstName,
            TypeMembre = type,
            SiteId = dto.SiteId
        };

        await membreRepository.AddAsync(membre);
        await unitOfWork.SaveChangesAsync();

        return ToDto(membre);
    }

    private async Task<Membre> GetMembreOrThrowAsync(int id)
    {
        Membre? membre = await membreRepository.GetByIdAsync(id);
        if (membre is null)
            throw new MembreNotFoundException(id);
        return membre;
    }

    private async Task GetSiteOrThrowAsync(int siteId)
    {
        Site? site = await siteRepository.GetByIdAsync(siteId);
        if (site is null)
            throw new SiteNotFoundException(siteId);
    }

    // ASSUMPTION: matricule format ([GSL]\d{1,5}) is already enforced by ContextExtensions.GetMatricule()
    // before this method is ever reached, so the default branch below is unreachable in practice.
    private static TypeMembre ResolveTypeMembre(string matricule) => matricule[0] switch
    {
        'G' => TypeMembre.Global,
        'S' => TypeMembre.Site,
        'L' => TypeMembre.Libre,
        _ => throw new ArgumentException($"Préfixe de matricule inconnu : {matricule}", nameof(matricule))
    };

    private static MembreDto ToDto(Membre membre) => new(
        membre.Id, membre.Matricule, membre.Name, membre.FirstName,
        membre.TypeMembre.ToString(), membre.SiteId, membre.SoldeDu, membre.DateFinPenalite);
}
