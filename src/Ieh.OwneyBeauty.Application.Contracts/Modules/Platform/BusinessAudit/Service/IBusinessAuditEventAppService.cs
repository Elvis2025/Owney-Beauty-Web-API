using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Ieh.OwneyBeauty.Modules.Platform.BusinessAudit.Dto;

namespace Ieh.OwneyBeauty.Modules.Platform.BusinessAudit.Service;

/// <summary>Consulta de auditoría de negocio (OB-49-002). Solo lectura.</summary>
public interface IBusinessAuditEventAppService : IApplicationService
{
    Task<PagedResultDto<BusinessAuditEventDto>> GetListAsync(GetBusinessAuditEventsInput input);
}
