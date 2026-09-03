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

    public async Task<MembreDto> CreateMembreAsync(string matricule, CreateMembreDto dto)
    {
        await createValidator.ValidateOrThrowAsync(dto);

        string typeMembreCode = ResolveTypeMembreCode(matricule);
        bool siteIdProvidedConsistently = (typeMembreCode == TypeMembreSeed.SiteCode) == (dto.SiteId is not null);
        if (!siteIdProvidedConsistently)
            throw new MembreSiteIdInvalideException();
        if (dto.SiteId is not null)
            await GetSiteOrThrowAsync(dto.SiteId.Value);

        TypeMembre typeMembre = await GetTypeMembreOrThrowAsync(typeMembreCode);

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

        // BUG FIX: don't set membre.TypeMembre = typeMembre above — typeMembre comes from an
        // AsNoTracking() query, so attaching it as a navigation on a newly-Added Membre made EF's
        // graph-walk mark that already-seeded TypeMembre row as Added too, causing a duplicate-PK
        // conflict on every member creation. Passing the code straight through avoids the navigation
        // entirely for this DTO.
        return ToDto(membre, typeMembre.Code);
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

    // ASSUMPTION: matricule format ([GSL]\d{1,5}) is already enforced by ContextExtensions.GetMatricule()
    // before this method is ever reached, so the default branch below is unreachable in practice.
    private static string ResolveTypeMembreCode(string matricule) => matricule[0] switch
    {
        'G' => TypeMembreSeed.GlobalCode,
        'S' => TypeMembreSeed.SiteCode,
        'L' => TypeMembreSeed.LibreCode,
        _ => throw new ArgumentException($"Préfixe de matricule inconnu : {matricule}", nameof(matricule))
    };

    private static MembreDto ToDto(Membre membre) => ToDto(membre, membre.TypeMembre!.Code);

    private static MembreDto ToDto(Membre membre, string typeMembreCode) => new(
        membre.Id, membre.Matricule, membre.Name, membre.FirstName,
        typeMembreCode,
        membre.SiteId,
        membre.SoldesDus.Where(s => s.Statut == StatutSoldeDu.Du).Sum(s => s.Montant),
        membre.Penalites.Where(p => p.Active).Select(p => (DateTime?)p.DateFin.ToDateTime(TimeOnly.MinValue)).Max());
}
