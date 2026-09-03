using Bll.Extensions;
using Core.Constants;
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
    ITypeMembreRepository typeMembreRepository,
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

    public async Task<IEnumerable<string>> GetAllMatriculesAsync()
    {
        return await membreRepository.GetAllMatriculesAsync();
    }

    public async Task<MembreDto> CreateMembreAsync(CreateMembreDto dto)
    {
        await createValidator.ValidateOrThrowAsync(dto);

        bool siteIdProvidedConsistently = (dto.Type == TypeMembreSeed.SiteCode) == (dto.SiteId is not null);
        if (!siteIdProvidedConsistently)
            throw new MembreSiteIdInvalideException();
        if (dto.SiteId is not null)
            await GetSiteOrThrowAsync(dto.SiteId.Value);

        TypeMembre typeMembre = await GetTypeMembreOrThrowAsync(dto.Type);
        string matricule = await GenererMatriculeAsync(typeMembre);

        var membre = new Membre
        {
            Matricule = matricule,
            Name = dto.Name,
            FirstName = dto.FirstName,
            TypeMembreId = typeMembre.Id,
            SiteId = dto.SiteId
        };

        await membreRepository.AddAsync(membre);
        await unitOfWork.SaveChangesAsync();

        
        
        
        
        
        return ToDto(membre, typeMembre.Code);
    }

    
    
    
    
    private async Task<string> GenererMatriculeAsync(TypeMembre typeMembre)
    {
        IEnumerable<string> matriculesExistants = await membreRepository.GetMatriculesByPrefixAsync(typeMembre.PrefixeMatricule);
        int prochainNumero = matriculesExistants
            .Select(m => int.TryParse(m[typeMembre.PrefixeMatricule.Length..], out int numero) ? numero : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;
        return $"{typeMembre.PrefixeMatricule}{prochainNumero}";
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

    private async Task<TypeMembre> GetTypeMembreOrThrowAsync(string code)
    {
        TypeMembre? typeMembre = await typeMembreRepository.GetByCodeAsync(code);
        if (typeMembre is null)
            throw new TypeMembreNotFoundByCodeException(code);
        return typeMembre;
    }

    private static MembreDto ToDto(Membre membre) => ToDto(membre, membre.TypeMembre!.Code);

    private static MembreDto ToDto(Membre membre, string typeMembreCode) => new(
        membre.Id, membre.Matricule, membre.Name, membre.FirstName,
        typeMembreCode,
        membre.SiteId,
        membre.SoldesDus.Where(s => s.Statut == StatutSoldeDu.Du).Sum(s => s.Montant),
        membre.Penalites.Where(p => p.Active).Select(p => (DateTime?)p.DateFin.ToDateTime(TimeOnly.MinValue)).Max());
}
