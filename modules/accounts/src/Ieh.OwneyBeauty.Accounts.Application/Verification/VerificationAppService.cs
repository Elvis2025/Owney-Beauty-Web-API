using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ieh.OwneyBeauty.SharedKernel.Auditing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace Ieh.OwneyBeauty.Accounts.Verification;

[Authorize]
public class VerificationAppService : AccountsAppService, IVerificationAppService
{
    private readonly IdentityUserManager _userManager;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly VerificationCodeManager _codeManager;
    private readonly VerificationSender _sender;
    private readonly IDistributedEventBus _eventBus;
    private readonly IBusinessAuditLogger _audit;

    public VerificationAppService(
        IdentityUserManager userManager,
        IRepository<IdentityUser, Guid> userRepository,
        VerificationCodeManager codeManager,
        VerificationSender sender,
        IDistributedEventBus eventBus,
        IBusinessAuditLogger audit)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _codeManager = codeManager;
        _sender = sender;
        _eventBus = eventBus;
        _audit = audit;
    }

    public virtual async Task<VerificationCodeSentDto> SendEmailCodeAsync()
    {
        var user = await _userManager.GetByIdAsync(CurrentUser.GetId());
        if (PlaceholderEmail.Is(user.Email))
        {
            throw new BusinessException(AccountsErrorCodes.NoEmailToVerify);
        }

        if (user.EmailConfirmed)
        {
            throw new BusinessException(AccountsErrorCodes.AlreadyVerified);
        }

        return await _sender.SendAsync(user, VerificationChannel.Email, user.Email);
    }

    public virtual async Task ConfirmEmailAsync(ConfirmCodeDto input)
    {
        var user = await _userManager.GetByIdAsync(CurrentUser.GetId());
        var challenge = await _codeManager.VerifyAsync(user.Id, VerificationChannel.Email, input.Code);
        if (!string.Equals(challenge.Target, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            // El correo cambió después de emitir el código: el código ya no aplica.
            throw new BusinessException(AccountsErrorCodes.VerificationCodeExpired);
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        (await _userManager.ConfirmEmailAsync(user, token)).CheckErrors();

        await _eventBus.PublishAsync(new ContactVerifiedEto { UserId = user.Id, Channel = VerificationChannel.Email });
        await _audit.LogAsync(new BusinessAuditEntry("Accounts", "Email.Verified", nameof(IdentityUser), user.Id.ToString()));
    }

    public virtual async Task<VerificationCodeSentDto> SendPhoneCodeAsync(SendPhoneCodeDto input)
    {
        var user = await _userManager.GetByIdAsync(CurrentUser.GetId());
        var phone = input.PhoneNumber?.Trim();

        if (!phone.IsNullOrWhiteSpace() && phone != user.PhoneNumber)
        {
            if (await _userRepository.AnyAsync(x => x.PhoneNumber == phone && x.Id != user.Id))
            {
                // Genérico: no revelar que el número pertenece a otra cuenta.
                throw new BusinessException(AccountsErrorCodes.RegistrationFailed);
            }

            (await _userManager.SetPhoneNumberAsync(user, phone)).CheckErrors();
        }

        if (user.PhoneNumber.IsNullOrWhiteSpace())
        {
            throw new BusinessException(AccountsErrorCodes.PhoneNumberInvalid);
        }

        if (user.PhoneNumberConfirmed)
        {
            throw new BusinessException(AccountsErrorCodes.AlreadyVerified);
        }

        return await _sender.SendAsync(user, VerificationChannel.Phone, user.PhoneNumber!);
    }

    public virtual async Task ConfirmPhoneAsync(ConfirmCodeDto input)
    {
        var user = await _userManager.GetByIdAsync(CurrentUser.GetId());
        var challenge = await _codeManager.VerifyAsync(user.Id, VerificationChannel.Phone, input.Code);
        if (user.PhoneNumber.IsNullOrWhiteSpace() || challenge.Target != user.PhoneNumber)
        {
            throw new BusinessException(AccountsErrorCodes.VerificationCodeExpired);
        }

        var token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber!);
        (await _userManager.ChangePhoneNumberAsync(user, user.PhoneNumber!, token)).CheckErrors();

        await _eventBus.PublishAsync(new ContactVerifiedEto { UserId = user.Id, Channel = VerificationChannel.Phone });
        await _audit.LogAsync(new BusinessAuditEntry("Accounts", "Phone.Verified", nameof(IdentityUser), user.Id.ToString(),
            Data: new Dictionary<string, string> { ["phone"] = ContactMasking.Mask(user.PhoneNumber!) }));
    }
}
