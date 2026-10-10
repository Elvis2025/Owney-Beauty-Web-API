using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Ieh.OwneyBeauty.Modules.Accounts.MyAccount.Dto;

namespace Ieh.OwneyBeauty.Modules.Accounts.MyAccount.Service;

/// <summary>Cuenta del usuario autenticado: modos, onboarding, documentos legales y consentimientos.</summary>
public interface IMyAccountAppService : IApplicationService
{
    Task<MyAccountDto> GetAsync();

    /// <summary>Cambia de modo sin cerrar sesión (OB-02-003). No concede permisos.</summary>
    Task<MyAccountDto> SwitchModeAsync(SwitchModeDto input);

    /// <summary>Habilita un modo adicional en la misma cuenta (cuenta híbrida, OB-02-002).</summary>
    Task<MyAccountDto> EnableModeAsync(EnableModeDto input);

    Task<MyAccountDto> AdvanceOnboardingAsync(AdvanceOnboardingDto input);

    Task<MyAccountDto> AcceptLegalDocumentsAsync(AcceptLegalDocumentsDto input);

    Task<ListResultDto<ConsentStatusDto>> GetConsentsAsync();

    Task<ConsentStatusDto> UpdateConsentAsync(UpdateConsentDto input);
}
