using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Emailing;
using Volo.Abp.EventBus;
using Volo.Abp.Identity;
using Ieh.OwneyBeauty.Localization;

namespace Ieh.OwneyBeauty.Modules.Accounts.Handlers;

/// <summary>
/// Alertas de seguridad por correo (ACC-F09, §13.1): cambio de contraseña, correo, teléfono,
/// MFA y bloqueo por intentos. Se basa en los security logs que ABP Identity ya registra.
/// El envío se encola como background job (lo procesa el Worker).
/// </summary>
public class SecurityAlertHandler : ILocalEventHandler<EntityCreatedEventData<IdentitySecurityLog>>, ITransientDependency
{
    private static readonly HashSet<string> AlertActions = new(StringComparer.Ordinal)
    {
        IdentitySecurityLogActionConsts.ChangePassword,
        IdentitySecurityLogActionConsts.ChangeEmail,
        IdentitySecurityLogActionConsts.ChangePhoneNumber,
        IdentitySecurityLogActionConsts.TwoFactorEnabled,
        IdentitySecurityLogActionConsts.TwoFactorDisabled,
        IdentitySecurityLogActionConsts.LoginLockedout,
    };

    private readonly IIdentityUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly IStringLocalizer<OwneyBeautyResource> _l;
    private readonly ILogger<SecurityAlertHandler> _logger;

    public SecurityAlertHandler(
        IIdentityUserRepository userRepository,
        IEmailSender emailSender,
        IStringLocalizer<OwneyBeautyResource> localizer,
        ILogger<SecurityAlertHandler> logger)
    {
        _userRepository = userRepository;
        _emailSender = emailSender;
        _l = localizer;
        _logger = logger;
    }

    public virtual async Task HandleEventAsync(EntityCreatedEventData<IdentitySecurityLog> eventData)
    {
        var log = eventData.Entity;
        if (log.UserId is null || log.Action is null || !AlertActions.Contains(log.Action))
        {
            return;
        }

        try
        {
            var user = await _userRepository.FindAsync(log.UserId.Value, includeDetails: false);
            if (user is null || string.IsNullOrWhiteSpace(user.Email) || !user.EmailConfirmed)
            {
                return;
            }

            var action = _l["SecurityAction:" + log.Action];
            var when = log.CreationTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            await _emailSender.QueueAsync(user.Email, _l["Email:SecurityAlertSubject"], _l["Email:SecurityAlertBody", action, when], isBodyHtml: false);
        }
        catch (Exception ex)
        {
            // Una alerta fallida nunca debe romper la operación de seguridad que la originó.
            _logger.LogWarning(ex, "No se pudo encolar la alerta de seguridad {Action} para {UserId}", log.Action, log.UserId);
        }
    }
}
