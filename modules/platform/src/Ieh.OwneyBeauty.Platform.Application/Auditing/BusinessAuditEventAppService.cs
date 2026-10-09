using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Ieh.OwneyBeauty.Platform.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Ieh.OwneyBeauty.Platform.Auditing;

[Authorize(PlatformPermissions.BusinessAudit.View)]
public class BusinessAuditEventAppService : PlatformAppService, IBusinessAuditEventAppService
{
    private readonly IRepository<BusinessAuditEvent, Guid> _repository;

    public BusinessAuditEventAppService(IRepository<BusinessAuditEvent, Guid> repository)
    {
        _repository = repository;
    }

    public virtual async Task<PagedResultDto<BusinessAuditEventDto>> GetListAsync(GetBusinessAuditEventsInput input)
    {
        var query = (await _repository.GetQueryableAsync())
            .WhereIf(!input.Module.IsNullOrWhiteSpace(), x => x.Module == input.Module)
            .WhereIf(!input.AuditAction.IsNullOrWhiteSpace(), x => x.Action == input.AuditAction)
            .WhereIf(!input.EntityType.IsNullOrWhiteSpace(), x => x.EntityType == input.EntityType)
            .WhereIf(!input.EntityId.IsNullOrWhiteSpace(), x => x.EntityId == input.EntityId)
            .WhereIf(input.UserId.HasValue, x => x.UserId == input.UserId)
            .WhereIf(input.Succeeded.HasValue, x => x.Succeeded == input.Succeeded)
            .WhereIf(input.From.HasValue, x => x.CreationTime >= input.From)
            .WhereIf(input.To.HasValue, x => x.CreationTime < input.To);

        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query
            .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? "CreationTime desc" : input.Sorting!)
            .PageBy(input));
        return new PagedResultDto<BusinessAuditEventDto>(total, ObjectMapper.Map<List<BusinessAuditEvent>, List<BusinessAuditEventDto>>(items));
    }
}
