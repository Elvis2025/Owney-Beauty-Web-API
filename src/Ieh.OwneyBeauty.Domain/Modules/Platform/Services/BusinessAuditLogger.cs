using System;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Auditing;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Volo.Abp.Tracing;
using Volo.Abp.Users;
using Ieh.OwneyBeauty.Common.Auditing;
using Ieh.OwneyBeauty.Modules.Platform.Models;

namespace Ieh.OwneyBeauty.Modules.Platform.Services;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IBusinessAuditLogger))]
public class BusinessAuditLogger : IBusinessAuditLogger, ITransientDependency
{
    private readonly IRepository<BusinessAuditEvent, Guid> _repository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICorrelationIdProvider _correlationIdProvider;
    private readonly IAuditingManager _auditingManager;

    public BusinessAuditLogger(
        IRepository<BusinessAuditEvent, Guid> repository,
        ICurrentUser currentUser,
        IClock clock,
        IGuidGenerator guidGenerator,
        ICorrelationIdProvider correlationIdProvider,
        IAuditingManager auditingManager)
    {
        _repository = repository;
        _currentUser = currentUser;
        _clock = clock;
        _guidGenerator = guidGenerator;
        _correlationIdProvider = correlationIdProvider;
        _auditingManager = auditingManager;
    }

    public virtual async Task LogAsync(BusinessAuditEntry entry)
    {
        var dataJson = entry.Data is { Count: > 0 } ? JsonSerializer.Serialize(entry.Data) : null;

        await _repository.InsertAsync(new BusinessAuditEvent(
            _guidGenerator.Create(),
            entry.Module,
            entry.Action,
            entry.EntityType,
            entry.EntityId,
            entry.Succeeded,
            entry.Reason,
            dataJson,
            _currentUser.Id,
            _currentUser.UserName,
            _auditingManager.Current?.Log.ClientIpAddress,
            _correlationIdProvider.Get(),
            _clock.Now));
    }
}
