using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Identity;
using IdentityUser = Volo.Abp.Identity.IdentityUser;
using Ieh.OwneyBeauty.Common.Auditing;
using Ieh.OwneyBeauty.Modules.Accounts.Enums;
using Ieh.OwneyBeauty.Modules.Accounts.Events;
using Ieh.OwneyBeauty.Modules.Accounts.Helpers;
using Ieh.OwneyBeauty.Modules.Accounts.Managers;
using Ieh.OwneyBeauty.Modules.Accounts.Models;
using Ieh.OwneyBeauty.Modules.Accounts.Registration.Dto;
using Ieh.OwneyBeauty.Modules.Accounts.Verification.Service;

namespace Ieh.OwneyBeauty.Modules.Accounts.Registration.Service;

[AllowAnonymous]
public class RegistrationAppService : OwneyBeautyAppService, IRegistrationAppService
{
    private static readonly Regex InvalidUserNameChars = new("[^a-z0-9._]", RegexOptions.Compiled);

    private readonly IdentityUserManager _userManager;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IRepository<AccountProfile, Guid> _profileRepository;
    private readonly IRepository<LegalDocument, Guid> _legalDocumentRepository;
    private readonly IRepository<ConsentRecord, Guid> _consentRepository;
    private readonly LegalDocumentManager _legalDocumentManager;
    private readonly VerificationSender _verificationSender;
    private readonly IDistributedEventBus _eventBus;
    private readonly IBusinessAuditLogger _audit;
    private readonly IAuditingManager _auditingManager;
    private readonly IOptions<IdentityOptions> _identityOptions;

    public RegistrationAppService(
        IdentityUserManager userManager,
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<AccountProfile, Guid> profileRepository,
        IRepository<LegalDocument, Guid> legalDocumentRepository,
        IRepository<ConsentRecord, Guid> consentRepository,
        LegalDocumentManager legalDocumentManager,
        VerificationSender verificationSender,
        IDistributedEventBus eventBus,
        IBusinessAuditLogger audit,
        IAuditingManager auditingManager,
        IOptions<IdentityOptions> identityOptions)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _legalDocumentRepository = legalDocumentRepository;
        _consentRepository = consentRepository;
        _legalDocumentManager = legalDocumentManager;
        _verificationSender = verificationSender;
        _eventBus = eventBus;
        _audit = audit;
        _auditingManager = auditingManager;
        _identityOptions = identityOptions;
    }

    public virtual async Task<RegistrationResultDto> RegisterAsync(RegisterDto input)
    {
        var email = input.Email?.Trim().ToLowerInvariant();
        var phone = input.PhoneNumber?.Trim();
        if (email.IsNullOrWhiteSpace() && phone.IsNullOrWhiteSpace())
        {
            throw new BusinessException(AccountsErrorCodes.EmailOrPhoneRequired);
        }

        if (input.InitialMode == AccountMode.Business)
        {
            throw new BusinessException(AccountsErrorCodes.ModeNotSupportedYet);
        }

        var acceptedDocuments = await ValidateLegalAcceptanceAsync(input);

        if (!phone.IsNullOrWhiteSpace() && await _userRepository.AnyAsync(x => x.PhoneNumber == phone))
        {
            // Mensaje genérico: no revelar si el teléfono ya tiene cuenta (OB-01-001).
            throw new BusinessException(AccountsErrorCodes.RegistrationFailed);
        }

        // Carga en IdentityOptions la política vigente (settings de ABP) antes de validar la contraseña.
        await _identityOptions.SetAsync();

        var userId = GuidGenerator.Create();
        var userName = await ResolveUserNameAsync(input.UserName, email, phone);
        var user = new IdentityUser(userId, userName, email ?? PlaceholderEmail.For(userId))
        {
            Name = input.Name.Trim(),
            Surname = input.Surname?.Trim(),
        };

        if (!phone.IsNullOrWhiteSpace())
        {
            user.SetPhoneNumber(phone, confirmed: false);
        }

        var result = await _userManager.CreateAsync(user, input.Password);
        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Code is "DuplicateEmail" or "InvalidEmail"))
            {
                Logger.LogInformation("Registro rechazado por correo duplicado o inválido.");
                throw new BusinessException(AccountsErrorCodes.RegistrationFailed);
            }

            if (result.Errors.Any(e => e.Code == "DuplicateUserName"))
            {
                throw new BusinessException(AccountsErrorCodes.UserNameTaken);
            }

            result.CheckErrors();
        }

        var roles = new List<string> { OwneyRoles.Client };
        if (input.InitialMode == AccountMode.Professional)
        {
            roles.Add(OwneyRoles.Professional);
        }

        (await _userManager.AddToRolesAsync(user, roles)).CheckErrors();

        await _profileRepository.InsertAsync(new AccountProfile(userId, input.InitialMode));
        await RecordConsentsAsync(userId, acceptedDocuments, input.MarketingConsent);

        await _eventBus.PublishAsync(new UserRegisteredEto
        {
            UserId = userId,
            UserName = userName,
            InitialMode = input.InitialMode,
            RegisteredAt = Clock.Now,
        });

        await _audit.LogAsync(new BusinessAuditEntry("Accounts", "User.Registered", nameof(IdentityUser), userId.ToString(),
            Data: new Dictionary<string, string> { ["mode"] = input.InitialMode.ToString() }));

        await CurrentUnitOfWork!.SaveChangesAsync();

        // Se envía un código automáticamente por el canal principal. El otro canal se verifica a demanda.
        if (!email.IsNullOrWhiteSpace())
        {
            await _verificationSender.SendAsync(user, VerificationChannel.Email, email!);
        }
        else
        {
            await _verificationSender.SendAsync(user, VerificationChannel.Phone, phone!);
        }

        return new RegistrationResultDto
        {
            UserId = userId,
            UserName = userName,
            ActiveMode = input.InitialMode,
            RequiresEmailVerification = !email.IsNullOrWhiteSpace(),
            RequiresPhoneVerification = !phone.IsNullOrWhiteSpace(),
        };
    }

    private async Task<List<LegalDocument>> ValidateLegalAcceptanceAsync(RegisterDto input)
    {
        var required = await _legalDocumentManager.GetCurrentRequiredAsync(input.Language);
        var acceptedIds = input.AcceptedLegalDocumentIds.ToHashSet();
        if (required.Any(doc => !acceptedIds.Contains(doc.Id)))
        {
            throw new BusinessException(AccountsErrorCodes.LegalDocumentsNotAccepted);
        }

        if (acceptedIds.Count == 0)
        {
            return new List<LegalDocument>();
        }

        // Solo se registran aceptaciones de documentos publicados y vigentes.
        return await _legalDocumentRepository.GetListAsync(x => acceptedIds.Contains(x.Id) && x.IsCurrent);
    }

    private async Task RecordConsentsAsync(Guid userId, List<LegalDocument> documents, bool marketing)
    {
        var now = Clock.Now;
        var ip = _auditingManager.Current?.Log.ClientIpAddress;
        foreach (var doc in documents)
        {
            await _consentRepository.InsertAsync(ConsentRecord.ForDocument(GuidGenerator.Create(), userId, doc, now, ip));
        }

        await _consentRepository.InsertAsync(ConsentRecord.ForConsent(GuidGenerator.Create(), userId, ConsentType.DataProcessing, true, now, ip));
        await _consentRepository.InsertAsync(ConsentRecord.ForConsent(GuidGenerator.Create(), userId, ConsentType.Marketing, marketing, now, ip));
    }

    private async Task<string> ResolveUserNameAsync(string? requested, string? email, string? phone)
    {
        if (!requested.IsNullOrWhiteSpace())
        {
            var name = requested!.Trim();
            if (await _userManager.FindByNameAsync(name) is not null)
            {
                throw new BusinessException(AccountsErrorCodes.UserNameTaken);
            }

            return name;
        }

        var seed = !email.IsNullOrWhiteSpace() ? email![..email!.IndexOf('@')] : "owney";
        seed = InvalidUserNameChars.Replace(seed.ToLowerInvariant(), string.Empty);
        if (seed.Length < 3)
        {
            seed = "owney";
        }

        seed = seed.Length > 20 ? seed[..20] : seed;
        for (var i = 0; i < 10; i++)
        {
            var candidate = $"{seed}{RandomNumberGenerator.GetInt32(1000, 10000)}";
            if (await _userManager.FindByNameAsync(candidate) is null)
            {
                return candidate;
            }
        }

        return $"{seed}{GuidGenerator.Create():N}"[..Math.Min(AccountsConsts.MaxUserNameLength, seed.Length + 12)];
    }
}
