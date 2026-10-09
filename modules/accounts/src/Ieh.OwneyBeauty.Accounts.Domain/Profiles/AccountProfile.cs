using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Ieh.OwneyBeauty.Accounts.Profiles;

/// <summary>
/// Estado de cuenta de Owney asociado 1:1 a un IdentityUser (Id = UserId).
/// Separa identidad (ABP Identity) de modo activo y onboarding (OB-02-001..003).
/// </summary>
public class AccountProfile : AuditedAggregateRoot<Guid>
{
    public AccountMode InitialMode { get; private set; }

    public AccountMode ActiveMode { get; private set; }

    public OnboardingStep OnboardingStep { get; private set; }

    protected AccountProfile()
    {
    }

    public AccountProfile(Guid userId, AccountMode initialMode)
        : base(userId)
    {
        InitialMode = initialMode;
        ActiveMode = initialMode;
        OnboardingStep = OnboardingStep.Profile;
    }

    /// <summary>
    /// Cambia el modo solo si la cuenta lo tiene disponible (según sus roles).
    /// Nunca concede permisos: los permisos los dan los roles, no el modo.
    /// </summary>
    public AccountMode SwitchMode(AccountMode target, IReadOnlyCollection<AccountMode> availableModes)
    {
        if (!availableModes.Contains(target))
        {
            throw new BusinessException(AccountsErrorCodes.ModeNotAvailable).WithData("mode", target);
        }

        var previous = ActiveMode;
        ActiveMode = target;
        return previous;
    }

    /// <summary>El onboarding solo avanza; no retrocede.</summary>
    public void AdvanceOnboarding(OnboardingStep step)
    {
        if (step > OnboardingStep)
        {
            OnboardingStep = step;
        }
    }

    /// <summary>Modos disponibles derivados de los roles del usuario.</summary>
    public static IReadOnlyCollection<AccountMode> ModesFromRoles(IEnumerable<string> roles)
    {
        var set = new HashSet<string>(roles, StringComparer.OrdinalIgnoreCase);
        var modes = new List<AccountMode> { AccountMode.Client };
        if (set.Contains(OwneyRoles.Professional))
        {
            modes.Add(AccountMode.Professional);
        }

        if (set.Contains(OwneyRoles.BusinessOwner) || set.Contains(OwneyRoles.Employee))
        {
            modes.Add(AccountMode.Business);
        }

        return modes;
    }
}
