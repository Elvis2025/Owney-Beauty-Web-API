using System;
using Volo.Abp.Application.Dtos;

namespace Ieh.OwneyBeauty.Modules.Platform.BusinessAudit.Dto;

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
