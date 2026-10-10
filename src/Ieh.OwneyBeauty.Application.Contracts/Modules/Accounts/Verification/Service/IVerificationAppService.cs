using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Ieh.OwneyBeauty.Modules.Accounts.Verification.Dto;

namespace Ieh.OwneyBeauty.Modules.Accounts.Verification.Service;

/// <summary>Verificación de correo y teléfono del usuario autenticado (OB-01-002/003).</summary>
public interface IVerificationAppService : IApplicationService
{
    Task<VerificationCodeSentDto> SendEmailCodeAsync();
    Task ConfirmEmailAsync(ConfirmCodeDto input);
    Task<VerificationCodeSentDto> SendPhoneCodeAsync(SendPhoneCodeDto input);
    Task ConfirmPhoneAsync(ConfirmCodeDto input);
}
