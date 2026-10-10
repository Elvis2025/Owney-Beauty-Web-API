using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using IdentityUser = Volo.Abp.Identity.IdentityUser;
using Ieh.OwneyBeauty.Common.Auditing;
using Ieh.OwneyBeauty.Modules.Accounts.Enums;
using Ieh.OwneyBeauty.Modules.Accounts.Events;
using Ieh.OwneyBeauty.Modules.Accounts.Helpers;
using Ieh.OwneyBeauty.Modules.Accounts.LegalDocuments.Dto;
using Ieh.OwneyBeauty.Modules.Accounts.Managers;
using Ieh.OwneyBeauty.Modules.Accounts.Models;
using Ieh.OwneyBeauty.Modules.Accounts.MyAccount.Dto;

namespace Ieh.OwneyBeauty.Modules.Accounts.MyAccount.Service;

[Authorize]
public class MyAccountAppService : OwneyBeautyAppService, IMyAccountAppService
{
    private readonly IdentityUserManager _userManager;
    private readonly IRepository<AccountProfile, Guid> _profileRepository;
    private readonly IRepository<LegalDocument, Guid> _legalDocumentRepository;
    private readonly IRepository<ConsentRecord, Guid> _consentRepository;
    private readonly LegalDocumentManager _legalDocumentManager;
    private readonly IDistributedEventBus _eventBus;
    private readonly IBusinessAuditLogger _audit;
    private readonly IAuditingManager _auditingManager;

    public MyAccountAppService(
        IdentityUserManager userManager,
        IRepository<AccountProfile, Guid> profileRepository,
        IRepository<LegalDocument, Guid> legalDocumentRepository,
        IRepository<ConsentRecord, Guid> consentRepository,
        LegalDocumentManager legalDocumentManager,
        IDistributedEventBus eventBus,
        IBusinessAuditLogger audit,
        IAuditingManager auditingManager)
    {
        _userManager = userManager;
        _profileRepository = profileRepository;
        _legalDocumentRepository = legalDocumentRepository;
        _consentRepository = consentRepository;
        _legalDocumentManager = legalDocumentManager;
        _eventBus = eventBus;
        _audit = audit;
        _auditingManager = auditingManager;
    }

    public virtual async Task<MyAccountDto> GetAsync()
    {
        var (user, roles, profile) = await LoadAsync();
        return await BuildAsync(user, roles, profile);
    }

    public virtual async Task<MyAccountDto> SwitchModeAsync(SwitchModeDto input)
    {
        var (user, roles, profile) = await LoadAsync();
        var previous = profile.SwitchMode(input.Mode, AccountProfile.ModesFromRoles(roles));
        await _profileRepository.UpdateAsync(profile, autoSave: true);

        if (previous != input.Mode)
        {
            await _eventBus.PublishAsync(new AccountModeChangedEto { UserId = user.Id, PreviousMode = previous, NewMode = input.Mode });
        }

        return await BuildAsync(user, roles, profile);
    }

    public virtual async Task<MyAccountDto> EnableModeAsync(EnableModeDto input)
    {
        var (user, roles, profile) = await LoadAsync();
        switch (input.Mode)
        {
            case AccountMode.Client:
                break;
            case AccountMode.Professional when !roles.Contains(OwneyRoles.Professional):
                // El rol permite iniciar el onboarding profesional. Recibir reservas exige KYC aprobado (módulo VER).
                (await _userManager.AddToRoleAsync(user, OwneyRoles.Professional)).CheckErrors();
                roles = (await _userManager.GetRolesAsync(user)).ToList();
                await _audit.LogAsync(new BusinessAuditEntry("Accounts", "Mode.Enabled", nameof(IdentityUser), user.Id.ToString(),
                    Data: new Dictionary<string, string> { ["mode"] = input.Mode.ToString() }));
                break;
            case AccountMode.Professional:
                break;
            default:
                throw new BusinessException(AccountsErrorCodes.ModeNotSupportedYet);
        }

        return await BuildAsync(user, roles, profile);
    }

    public virtual async Task<MyAccountDto> AdvanceOnboardingAsync(AdvanceOnboardingDto input)
    {
        var (user, roles, profile) = await LoadAsync();
        profile.AdvanceOnboarding(input.Step);
        await _profileRepository.UpdateAsync(profile, autoSave: true);
        return await BuildAsync(user, roles, profile);
    }

    public virtual async Task<MyAccountDto> AcceptLegalDocumentsAsync(AcceptLegalDocumentsDto input)
    {
        var (user, roles, profile) = await LoadAsync();
        var ids = input.LegalDocumentIds.Distinct().ToList();
        var documents = await _legalDocumentRepository.GetListAsync(x => ids.Contains(x.Id) && x.IsCurrent);
        var now = Clock.Now;
        var ip = _auditingManager.Current?.Log.ClientIpAddress;
        foreach (var doc in documents)
        {
            await _consentRepository.InsertAsync(ConsentRecord.ForDocument(GuidGenerator.Create(), user.Id, doc, now, ip));
        }

        await CurrentUnitOfWork!.SaveChangesAsync();
        return await BuildAsync(user, roles, profile);
    }

    public virtual async Task<ListResultDto<ConsentStatusDto>> GetConsentsAsync()
    {
        var userId = CurrentUser.GetId();
        var records = await _consentRepository.GetListAsync(x => x.UserId == userId && x.ConsentType != null);
        var latest = records
            .GroupBy(x => x.ConsentType!.Value)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.CreationTime).First());

        var items = Enum.GetValues<ConsentType>()
            .Select(type => latest.TryGetValue(type, out var r)
                ? new ConsentStatusDto { Type = type, Granted = r.Granted, UpdatedAt = r.CreationTime }
                : new ConsentStatusDto { Type = type, Granted = false })
            .ToList();
        return new ListResultDto<ConsentStatusDto>(items);
    }

    public virtual async Task<ConsentStatusDto> UpdateConsentAsync(UpdateConsentDto input)
    {
        var userId = CurrentUser.GetId();
        var record = ConsentRecord.ForConsent(GuidGenerator.Create(), userId, input.Type, input.Granted, Clock.Now, _auditingManager.Current?.Log.ClientIpAddress);
        await _consentRepository.InsertAsync(record, autoSave: true);
        await _audit.LogAsync(new BusinessAuditEntry("Accounts", "Consent.Updated", "Consent", userId.ToString(),
            Data: new Dictionary<string, string> { ["type"] = input.Type.ToString(), ["granted"] = input.Granted.ToString() }));
        return new ConsentStatusDto { Type = input.Type, Granted = input.Granted, UpdatedAt = record.CreationTime };
    }

    private async Task<(IdentityUser User, List<string> Roles, AccountProfile Profile)> LoadAsync()
    {
        var user = await _userManager.GetByIdAsync(CurrentUser.GetId());
        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var profile = await _profileRepository.FindAsync(user.Id);
        if (profile is null)
        {
            // Usuarios creados fuera del registro de Owney (p. ej. por un admin) reciben su perfil al primer acceso.
            var mode = roles.Contains(OwneyRoles.Professional) ? AccountMode.Professional : AccountMode.Client;
            profile = await _profileRepository.InsertAsync(new AccountProfile(user.Id, mode), autoSave: true);
        }

        return (user, roles, profile);
    }

    private async Task<MyAccountDto> BuildAsync(IdentityUser user, List<string> roles, AccountProfile profile)
    {
        var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var pending = await _legalDocumentManager.GetPendingForUserAsync(user.Id, language);

        return new MyAccountDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Name = user.Name,
            Surname = user.Surname,
            Email = PlaceholderEmail.Public(user.Email),
            EmailConfirmed = user.EmailConfirmed && !PlaceholderEmail.Is(user.Email),
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            Roles = roles,
            AvailableModes = AccountProfile.ModesFromRoles(roles).ToList(),
            ActiveMode = profile.ActiveMode,
            OnboardingStep = profile.OnboardingStep,
            PendingLegalDocuments = ObjectMapper.Map<List<LegalDocument>, List<PublicLegalDocumentDto>>(pending),
        };
    }
}
