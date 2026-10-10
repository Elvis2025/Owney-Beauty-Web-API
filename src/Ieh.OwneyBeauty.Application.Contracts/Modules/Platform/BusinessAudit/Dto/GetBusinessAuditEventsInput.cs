using System;
using Volo.Abp.Application.Dtos;

namespace Ieh.OwneyBeauty.Modules.Platform.BusinessAudit.Dto;

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
