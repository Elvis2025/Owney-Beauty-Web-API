using System;
using Ieh.OwneyBeauty.Accounts;
using Ieh.OwneyBeauty.Accounts.Legal;
using Ieh.OwneyBeauty.Accounts.Profiles;
using Ieh.OwneyBeauty.Accounts.Verification;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ieh.OwneyBeauty.Owney;

public class VerificationChallengeTests
{
    private static readonly DateTime Now = new(2026, 10, 9, 12, 0, 0, DateTimeKind.Utc);

    private static VerificationChallenge Create(string code = "123456", int maxAttempts = 3) =>
        new(Guid.NewGuid(), Guid.NewGuid(), VerificationChannel.Email, "ana@example.com", code, Now, TimeSpan.FromMinutes(10), maxAttempts);

    [Fact]
    public void Never_stores_the_code_in_clear_text()
    {
        var challenge = Create("654321");
        challenge.CodeHash.ShouldNotContain("654321");
        challenge.CodeHash.Length.ShouldBe(64);
    }

    [Fact]
    public void Accepts_the_right_code_once()
    {
        var challenge = Create();
        challenge.Verify("123456", Now.AddMinutes(1)).ShouldBe(VerificationResult.Success);
        challenge.Verify("123456", Now.AddMinutes(2)).ShouldBe(VerificationResult.Expired);
    }

    [Fact]
    public void Expires_after_lifetime()
    {
        Create().Verify("123456", Now.AddMinutes(11)).ShouldBe(VerificationResult.Expired);
    }

    [Fact]
    public void Locks_after_max_attempts_even_with_right_code()
    {
        var challenge = Create(maxAttempts: 3);
        challenge.Verify("000000", Now).ShouldBe(VerificationResult.Invalid);
        challenge.Verify("111111", Now).ShouldBe(VerificationResult.Invalid);
        challenge.Verify("222222", Now).ShouldBe(VerificationResult.TooManyAttempts);
        challenge.Verify("123456", Now).ShouldBe(VerificationResult.TooManyAttempts);
    }

    [Fact]
    public void Invalidated_code_cannot_be_used()
    {
        var challenge = Create();
        challenge.Invalidate(Now);
        challenge.IsActiveAt(Now).ShouldBeFalse();
        challenge.Verify("123456", Now).ShouldBe(VerificationResult.Expired);
    }
}

public class AccountProfileTests
{
    [Fact]
    public void Modes_are_derived_from_roles()
    {
        AccountProfile.ModesFromRoles(new[] { OwneyRoles.Client }).ShouldBe(new[] { AccountMode.Client });
        AccountProfile.ModesFromRoles(new[] { OwneyRoles.Client, OwneyRoles.Professional })
            .ShouldBe(new[] { AccountMode.Client, AccountMode.Professional });
    }

    [Fact]
    public void Cannot_switch_to_a_mode_the_account_does_not_have()
    {
        var profile = new AccountProfile(Guid.NewGuid(), AccountMode.Client);
        var ex = Should.Throw<BusinessException>(() => profile.SwitchMode(AccountMode.Professional, new[] { AccountMode.Client }));
        ex.Code.ShouldBe(AccountsErrorCodes.ModeNotAvailable);
        profile.ActiveMode.ShouldBe(AccountMode.Client);
    }

    [Fact]
    public void Switching_returns_previous_mode()
    {
        var profile = new AccountProfile(Guid.NewGuid(), AccountMode.Client);
        profile.SwitchMode(AccountMode.Professional, new[] { AccountMode.Client, AccountMode.Professional }).ShouldBe(AccountMode.Client);
        profile.ActiveMode.ShouldBe(AccountMode.Professional);
    }

    [Fact]
    public void Onboarding_only_moves_forward()
    {
        var profile = new AccountProfile(Guid.NewGuid(), AccountMode.Client);
        profile.AdvanceOnboarding(OnboardingStep.Completed);
        profile.AdvanceOnboarding(OnboardingStep.Profile);
        profile.OnboardingStep.ShouldBe(OnboardingStep.Completed);
    }
}

public class LegalDocumentTests
{
    [Fact]
    public void Requires_content_or_url()
    {
        Should.Throw<ArgumentException>(() =>
            new LegalDocument(Guid.NewGuid(), LegalDocumentType.TermsOfService, "es", "1", "Términos", null, null, true));
    }

    [Fact]
    public void Draft_can_be_edited_but_consent_requires_publication()
    {
        var doc = new LegalDocument(Guid.NewGuid(), LegalDocumentType.PrivacyPolicy, "ES", "2026-10", "Privacidad", "texto", null, true);
        doc.Language.ShouldBe("es");
        doc.SetContent("Privacidad v2", "texto 2", null, true);
        doc.Title.ShouldBe("Privacidad v2");

        var ex = Should.Throw<BusinessException>(() =>
            ConsentRecord.ForDocument(Guid.NewGuid(), Guid.NewGuid(), doc, DateTime.UtcNow, "127.0.0.1"));
        ex.Code.ShouldBe(AccountsErrorCodes.LegalDocumentNotPublished);
    }
}
