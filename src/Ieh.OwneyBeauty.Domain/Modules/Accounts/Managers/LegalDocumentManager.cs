using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Ieh.OwneyBeauty.Modules.Accounts.Enums;
using Ieh.OwneyBeauty.Modules.Accounts.Models;

namespace Ieh.OwneyBeauty.Modules.Accounts.Managers;

public class LegalDocumentManager : DomainService
{
    private readonly IRepository<LegalDocument, Guid> _documentRepository;
    private readonly IRepository<ConsentRecord, Guid> _consentRepository;

    public LegalDocumentManager(IRepository<LegalDocument, Guid> documentRepository, IRepository<ConsentRecord, Guid> consentRepository)
    {
        _documentRepository = documentRepository;
        _consentRepository = consentRepository;
    }

    public virtual async Task<LegalDocument> CreateAsync(LegalDocumentType type, string language, string version, string title, string? content, string? externalUrl, bool requiresAcceptance)
    {
        var lang = language.Trim().ToLowerInvariant();
        var ver = version.Trim();
        if (await _documentRepository.AnyAsync(x => x.Type == type && x.Language == lang && x.Version == ver))
        {
            throw new BusinessException(AccountsErrorCodes.LegalDocumentVersionExists);
        }

        return new LegalDocument(GuidGenerator.Create(), type, lang, ver, title, content, externalUrl, requiresAcceptance);
    }

    /// <summary>Publica la versión y retira la vigente anterior del mismo tipo e idioma.</summary>
    public virtual async Task PublishAsync(LegalDocument document)
    {
        var previous = await _documentRepository.GetListAsync(x =>
            x.Type == document.Type && x.Language == document.Language && x.IsCurrent && x.Id != document.Id);
        foreach (var old in previous)
        {
            old.Supersede();
            await _documentRepository.UpdateAsync(old);
        }

        document.Publish(Clock.Now);
    }

    /// <summary>Documentos vigentes que exigen aceptación y el usuario aún no aceptó.</summary>
    public virtual async Task<List<LegalDocument>> GetPendingForUserAsync(Guid userId, string language)
    {
        var current = await GetCurrentRequiredAsync(language);
        if (current.Count == 0)
        {
            return current;
        }

        var ids = current.Select(x => x.Id).ToList();
        var accepted = (await _consentRepository.GetListAsync(x =>
                x.UserId == userId && x.LegalDocumentId != null && ids.Contains(x.LegalDocumentId.Value) && x.Granted))
            .Select(x => x.LegalDocumentId!.Value)
            .ToHashSet();

        return current.Where(x => !accepted.Contains(x.Id)).ToList();
    }

    public virtual async Task<List<LegalDocument>> GetCurrentRequiredAsync(string language)
    {
        var lang = language.Trim().ToLowerInvariant();
        var docs = await _documentRepository.GetListAsync(x => x.IsCurrent && x.RequiresAcceptance && x.Language == lang);
        if (docs.Count == 0 && lang != "es")
        {
            docs = await _documentRepository.GetListAsync(x => x.IsCurrent && x.RequiresAcceptance && x.Language == "es");
        }

        return docs;
    }
}
