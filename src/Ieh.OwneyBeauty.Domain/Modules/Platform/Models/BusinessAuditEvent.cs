using System;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;

namespace Ieh.OwneyBeauty.Modules.Platform.Models;

/// <summary>
/// Registro inmutable de una acción de negocio sensible (OB-49-001). No tiene métodos de modificación:
/// una vez escrito, solo se consulta.
/// </summary>
public class BusinessAuditEvent : Entity<Guid>, IHasCreationTime
{
    public string Module { get; private set; } = null!;

    public string Action { get; private set; } = null!;

    public string EntityType { get; private set; } = null!;

    public string? EntityId { get; private set; }

    public bool Succeeded { get; private set; }

    public string? Reason { get; private set; }

    /// <summary>Datos adicionales en JSON. Nunca debe contener secretos ni datos personales completos.</summary>
    public string? DataJson { get; private set; }

    public Guid? UserId { get; private set; }

    public string? UserName { get; private set; }

    public string? ClientIpAddress { get; private set; }

    public string? CorrelationId { get; private set; }

    public DateTime CreationTime { get; private set; }

    protected BusinessAuditEvent()
    {
    }

    public BusinessAuditEvent(
        Guid id,
        string module,
        string action,
        string entityType,
        string? entityId,
        bool succeeded,
        string? reason,
        string? dataJson,
        Guid? userId,
        string? userName,
        string? clientIpAddress,
        string? correlationId,
        DateTime creationTime)
        : base(id)
    {
        Module = Truncate(module, PlatformConsts.MaxAuditModuleLength)!;
        Action = Truncate(action, PlatformConsts.MaxAuditActionLength)!;
        EntityType = Truncate(entityType, PlatformConsts.MaxAuditEntityTypeLength)!;
        EntityId = Truncate(entityId, PlatformConsts.MaxAuditEntityIdLength);
        Succeeded = succeeded;
        Reason = Truncate(reason, PlatformConsts.MaxAuditReasonLength);
        DataJson = dataJson;
        UserId = userId;
        UserName = Truncate(userName, PlatformConsts.MaxAuditUserNameLength);
        ClientIpAddress = Truncate(clientIpAddress, PlatformConsts.MaxAuditClientIpLength);
        CorrelationId = Truncate(correlationId, PlatformConsts.MaxAuditCorrelationIdLength);
        CreationTime = creationTime;
    }

    private static string? Truncate(string? value, int max) =>
        value is null ? null : value.Length <= max ? value : value[..max];
}
