using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Ieh.OwneyBeauty.Modules.Accounts.Registration.Dto;

namespace Ieh.OwneyBeauty.Modules.Accounts.Registration.Service;

public interface IRegistrationAppService : IApplicationService
{
    /// <summary>Registro público (OB-01-001). Tras registrarse, el cliente inicia sesión por OAuth.</summary>
    Task<RegistrationResultDto> RegisterAsync(RegisterDto input);
}
