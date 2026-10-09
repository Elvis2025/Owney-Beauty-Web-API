using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Ieh.OwneyBeauty.Platform.Auditing;

public class BusinessAuditEventDto : EntityDto<Guid>
{
    public string Module { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public string? EntityId { get; set; }
    public bool Succeeded { get; set; }
    public string? Reason { get; set; }
    public string? DataJson { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? ClientIpAddress { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime CreationTime { get; set; }
}

public class GetBusinessAuditEventsInput : PagedAndSortedResultRequestDto
{
    public string? Module { get; set; }

    /// <summary>Acción auditada. No se llama "Action" porque MVC lo llenaría con el nombre de la acción de la ruta.</summary>
    public string? AuditAction { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public Guid? UserId { get; set; }
    public bool? Succeeded { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

/// <summary>Consulta de auditoría de negocio (OB-49-002). Solo lectura.</summary>
public interface IBusinessAuditEventAppService : IApplicationService
{
    Task<PagedResultDto<BusinessAuditEventDto>> GetListAsync(GetBusinessAuditEventsInput input);
}
