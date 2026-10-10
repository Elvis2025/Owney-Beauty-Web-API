using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Ieh.OwneyBeauty.Common.Auditing;
using Ieh.OwneyBeauty.Modules.Accounts.LegalDocuments.Dto;
using Ieh.OwneyBeauty.Modules.Accounts.Managers;
using Ieh.OwneyBeauty.Modules.Accounts.Models;

namespace Ieh.OwneyBeauty.Modules.Accounts.LegalDocuments.Service;

[Authorize(AccountsPermissions.LegalDocuments.Manage)]
public class LegalDocumentAppService : OwneyBeautyAppService, ILegalDocumentAppService
{
    private readonly IRepository<LegalDocument, Guid> _repository;
    private readonly LegalDocumentManager _manager;
    private readonly IBusinessAuditLogger _audit;

    public LegalDocumentAppService(IRepository<LegalDocument, Guid> repository, LegalDocumentManager manager, IBusinessAuditLogger audit)
    {
        _repository = repository;
        _manager = manager;
        _audit = audit;
    }

    [AllowAnonymous]
    public virtual async Task<ListResultDto<PublicLegalDocumentDto>> GetCurrentAsync(string language = "es")
    {
        var lang = (language ?? "es").Trim().ToLowerInvariant();
        var docs = await _repository.GetListAsync(x => x.IsCurrent && x.Language == lang);
        if (docs.Count == 0 && lang != "es")
        {
            docs = await _repository.GetListAsync(x => x.IsCurrent && x.Language == "es");
        }

        return new ListResultDto<PublicLegalDocumentDto>(
            ObjectMapper.Map<List<LegalDocument>, List<PublicLegalDocumentDto>>(docs.OrderBy(x => x.Type).ToList()));
    }

    public virtual async Task<PagedResultDto<LegalDocumentDto>> GetListAsync(GetLegalDocumentsInput input)
    {
        var query = (await _repository.GetQueryableAsync())
            .WhereIf(input.Type.HasValue, x => x.Type == input.Type)
            .WhereIf(!input.Language.IsNullOrWhiteSpace(), x => x.Language == input.Language!.ToLower())
            .WhereIf(input.IsCurrent.HasValue, x => x.IsCurrent == input.IsCurrent);

        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query
            .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? "Type, CreationTime desc" : input.Sorting!)
            .PageBy(input));
        return new PagedResultDto<LegalDocumentDto>(total, ObjectMapper.Map<List<LegalDocument>, List<LegalDocumentDto>>(items));
    }

    public virtual async Task<LegalDocumentDto> GetAsync(Guid id) =>
        ObjectMapper.Map<LegalDocument, LegalDocumentDto>(await _repository.GetAsync(id));

    public virtual async Task<LegalDocumentDto> CreateAsync(CreateLegalDocumentDto input)
    {
        var doc = await _manager.CreateAsync(input.Type, input.Language, input.Version, input.Title, input.Content, input.ExternalUrl, input.RequiresAcceptance);
        await _repository.InsertAsync(doc, autoSave: true);
        await _audit.LogAsync(new BusinessAuditEntry("Accounts", "LegalDocument.Created", nameof(LegalDocument), doc.Id.ToString()));
        return ObjectMapper.Map<LegalDocument, LegalDocumentDto>(doc);
    }

    public virtual async Task<LegalDocumentDto> UpdateAsync(Guid id, UpdateLegalDocumentDto input)
    {
        var doc = await _repository.GetAsync(id);
        doc.SetContent(input.Title, input.Content, input.ExternalUrl, input.RequiresAcceptance);
        await _repository.UpdateAsync(doc, autoSave: true);
        return ObjectMapper.Map<LegalDocument, LegalDocumentDto>(doc);
    }

    public virtual async Task<LegalDocumentDto> PublishAsync(Guid id)
    {
        var doc = await _repository.GetAsync(id);
        await _manager.PublishAsync(doc);
        await _repository.UpdateAsync(doc, autoSave: true);
        await _audit.LogAsync(new BusinessAuditEntry("Accounts", "LegalDocument.Published", nameof(LegalDocument), doc.Id.ToString(),
            Data: new Dictionary<string, string> { ["type"] = doc.Type.ToString(), ["version"] = doc.Version, ["language"] = doc.Language }));
        return ObjectMapper.Map<LegalDocument, LegalDocumentDto>(doc);
    }
}
