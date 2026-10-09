using System.Threading.Tasks;
using Ieh.OwneyBeauty.Accounts.Localization;
using Microsoft.Extensions.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Emailing;
using Volo.Abp.Settings;
using Volo.Abp.Sms;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace Ieh.OwneyBeauty.Accounts.Verification;

/// <summary>Emite un código y lo entrega por correo (encolado, lo envía el Worker) o SMS.</summary>
public class VerificationSender : ITransientDependency
{
    private readonly VerificationCodeManager _codeManager;
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly ISettingProvider _settingProvider;
    private readonly IStringLocalizer<AccountsResource> _l;

    public VerificationSender(
        VerificationCodeManager codeManager,
        IEmailSender emailSender,
        ISmsSender smsSender,
        ISettingProvider settingProvider,
        IStringLocalizer<AccountsResource> localizer)
    {
        _codeManager = codeManager;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _settingProvider = settingProvider;
        _l = localizer;
    }

    public virtual async Task<VerificationCodeSentDto> SendAsync(IdentityUser user, VerificationChannel channel, string target)
    {
        var (challenge, code) = await _codeManager.IssueAsync(user.Id, channel, target);
        var minutes = await _settingProvider.GetAsync(AccountsSettingNames.VerificationCodeLifetimeMinutes, 10);

        if (channel == VerificationChannel.Email)
        {
            await _emailSender.QueueAsync(target, _l["Email:VerificationSubject"], _l["Email:VerificationBody", code, minutes], isBodyHtml: false);
        }
        else
        {
            await _smsSender.SendAsync(new SmsMessage(target, _l["Sms:VerificationBody", code, minutes]));
        }

        return new VerificationCodeSentDto
        {
            Channel = channel,
            MaskedTarget = ContactMasking.Mask(target),
            ExpiresAt = challenge.ExpirationTime,
        };
    }
}
