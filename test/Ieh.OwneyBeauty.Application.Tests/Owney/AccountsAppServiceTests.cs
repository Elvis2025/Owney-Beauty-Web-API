using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Ieh.OwneyBeauty.Accounts;
using Ieh.OwneyBeauty.Accounts.Legal;
using Ieh.OwneyBeauty.Accounts.MyAccount;
using Ieh.OwneyBeauty.Accounts.Registration;
using Ieh.OwneyBeauty.Accounts.Verification;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Xunit;

namespace Ieh.OwneyBeauty.Owney;

public abstract class AccountsAppServiceTests<TStartupModule> : OwneyBeautyApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IRegistrationAppService _registration;
    private readonly IMyAccountAppService _myAccount;
    private readonly IVerificationAppService _verification;
    private readonly ILegalDocumentAppService _legalDocuments;
    private readonly IdentityUserManager _userManager;
    private readonly ICurrentPrincipalAccessor _principalAccessor;

    protected AccountsAppServiceTests()
    {
        _registration = GetRequiredService<IRegistrationAppService>();
        _myAccount = GetRequiredService<IMyAccountAppService>();
        _verification = GetRequiredService<IVerificationAppService>();
        _legalDocuments = GetRequiredService<ILegalDocumentAppService>();
        _userManager = GetRequiredService<IdentityUserManager>();
        _principalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    private async Task<List<Guid>> PublishTermsAsync()
    {
        var current = await _legalDocuments.GetCurrentAsync("es");
        var existing = current.Items.Where(x => x.RequiresAcceptance).Select(x => x.Id).ToList();
        if (existing.Count > 0)
        {
            return existing;
        }

        var draft = await _legalDocuments.CreateAsync(new CreateLegalDocumentDto
        {
            Type = LegalDocumentType.TermsOfService,
            Language = "es",
            Version = "test-" + Guid.NewGuid().ToString("N")[..6],
            Title = "Términos",
            Content = "Contenido",
            RequiresAcceptance = true,
        });
        var published = await _legalDocuments.PublishAsync(draft.Id);
        published.IsCurrent.ShouldBeTrue();
        return new List<Guid> { published.Id };
    }

    private static RegisterDto NewRegistration(List<Guid> accepted) => new()
    {
        Email = $"qa.{Guid.NewGuid().ToString("N")[..12]}@example.com",
        Name = "Ana",
        Password = "Owney2026",
        InitialMode = AccountMode.Client,
        AcceptedLegalDocumentIds = accepted,
        MarketingConsent = true,
    };

    private IDisposable LoginAs(Guid userId, string userName) =>
        _principalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.UserName, userName),
        }, "test")));

    [Fact]
    public async Task Registration_requires_accepting_current_legal_documents()
    {
        await PublishTermsAsync();
        var ex = await Should.ThrowAsync<BusinessException>(() => _registration.RegisterAsync(NewRegistration(new List<Guid>())));
        ex.Code.ShouldBe(AccountsErrorCodes.LegalDocumentsNotAccepted);
    }

    [Fact]
    public async Task Registration_creates_user_with_client_role_and_profile()
    {
        var accepted = await PublishTermsAsync();
        var result = await _registration.RegisterAsync(NewRegistration(accepted));

        result.RequiresEmailVerification.ShouldBeTrue();
        var user = await _userManager.GetByIdAsync(result.UserId);
        (await _userManager.GetRolesAsync(user)).ShouldContain(OwneyRoles.Client);

        using (LoginAs(result.UserId, result.UserName))
        {
            var me = await _myAccount.GetAsync();
            me.ActiveMode.ShouldBe(AccountMode.Client);
            me.EmailConfirmed.ShouldBeFalse();
            me.PendingLegalDocuments.ShouldBeEmpty();
        }
    }

    [Fact]
    public async Task Duplicate_email_returns_generic_error()
    {
        var accepted = await PublishTermsAsync();
        var input = NewRegistration(accepted);
        await _registration.RegisterAsync(input);

        var ex = await Should.ThrowAsync<BusinessException>(() => _registration.RegisterAsync(input));
        ex.Code.ShouldBe(AccountsErrorCodes.RegistrationFailed);
    }

    [Fact]
    public async Task Hybrid_account_can_enable_and_switch_to_professional_mode()
    {
        var accepted = await PublishTermsAsync();
        var result = await _registration.RegisterAsync(NewRegistration(accepted));

        using (LoginAs(result.UserId, result.UserName))
        {
            var ex = await Should.ThrowAsync<BusinessException>(() => _myAccount.SwitchModeAsync(new SwitchModeDto { Mode = AccountMode.Professional }));
            ex.Code.ShouldBe(AccountsErrorCodes.ModeNotAvailable);

            var enabled = await _myAccount.EnableModeAsync(new EnableModeDto { Mode = AccountMode.Professional });
            enabled.AvailableModes.ShouldContain(AccountMode.Professional);

            var switched = await _myAccount.SwitchModeAsync(new SwitchModeDto { Mode = AccountMode.Professional });
            switched.ActiveMode.ShouldBe(AccountMode.Professional);
        }
    }

    [Fact]
    public async Task Verification_code_cannot_be_resent_immediately()
    {
        var accepted = await PublishTermsAsync();
        var result = await _registration.RegisterAsync(NewRegistration(accepted));

        using (LoginAs(result.UserId, result.UserName))
        {
            var ex = await Should.ThrowAsync<BusinessException>(() => _verification.SendEmailCodeAsync());
            ex.Code.ShouldBe(AccountsErrorCodes.VerificationResendTooSoon);
        }
    }

    [Fact]
    public async Task Marketing_consent_can_be_withdrawn()
    {
        var accepted = await PublishTermsAsync();
        var result = await _registration.RegisterAsync(NewRegistration(accepted));

        using (LoginAs(result.UserId, result.UserName))
        {
            (await _myAccount.GetConsentsAsync()).Items.Single(x => x.Type == ConsentType.Marketing).Granted.ShouldBeTrue();
            await _myAccount.UpdateConsentAsync(new UpdateConsentDto { Type = ConsentType.Marketing, Granted = false });
            (await _myAccount.GetConsentsAsync()).Items.Single(x => x.Type == ConsentType.Marketing).Granted.ShouldBeFalse();
        }
    }
}
