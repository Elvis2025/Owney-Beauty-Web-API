using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Settings;
using Ieh.OwneyBeauty.Modules.Accounts.Enums;
using Ieh.OwneyBeauty.Modules.Accounts.Models;

namespace Ieh.OwneyBeauty.Modules.Accounts.Managers;

/// <summary>
/// Emite y valida códigos de verificación aplicando: expiración, límite de intentos,
/// intervalo mínimo entre reenvíos, límite diario e invalidación de códigos anteriores.
/// </summary>
public class VerificationCodeManager : DomainService
{
    private readonly IRepository<VerificationChallenge, Guid> _repository;
    private readonly ISettingProvider _settingProvider;

    public VerificationCodeManager(IRepository<VerificationChallenge, Guid> repository, ISettingProvider settingProvider)
    {
        _repository = repository;
        _settingProvider = settingProvider;
    }

    /// <summary>Crea un código nuevo y devuelve el código en claro para enviarlo (nunca se persiste en claro).</summary>
    public virtual async Task<(VerificationChallenge Challenge, string Code)> IssueAsync(Guid userId, VerificationChannel channel, string target)
    {
        var now = Clock.Now;
        var lifetime = TimeSpan.FromMinutes(await _settingProvider.GetAsync(AccountsSettingNames.VerificationCodeLifetimeMinutes, 10));
        var maxAttempts = await _settingProvider.GetAsync(AccountsSettingNames.VerificationMaxAttempts, 5);
        var resendSeconds = await _settingProvider.GetAsync(AccountsSettingNames.VerificationResendIntervalSeconds, 60);
        var dailyLimit = await _settingProvider.GetAsync(AccountsSettingNames.VerificationDailyLimit, 10);

        var recent = await _repository.GetListAsync(x => x.UserId == userId && x.Channel == channel && x.CreationTime > now.AddDays(-1));

        if (recent.Count >= dailyLimit)
        {
            throw new BusinessException(AccountsErrorCodes.VerificationDailyLimitReached);
        }

        var last = recent.OrderByDescending(x => x.CreationTime).FirstOrDefault();
        if (last is not null)
        {
            var wait = last.CreationTime.AddSeconds(resendSeconds) - now;
            if (wait > TimeSpan.Zero)
            {
                throw new BusinessException(AccountsErrorCodes.VerificationResendTooSoon)
                    .WithData("seconds", (int)Math.Ceiling(wait.TotalSeconds));
            }
        }

        foreach (var active in recent.Where(x => x.IsActiveAt(now)))
        {
            active.Invalidate(now);
            await _repository.UpdateAsync(active);
        }

        var code = GenerateNumericCode(AccountsConsts.VerificationCodeLength);
        var challenge = new VerificationChallenge(GuidGenerator.Create(), userId, channel, target, code, now, lifetime, maxAttempts);
        await _repository.InsertAsync(challenge);
        return (challenge, code);
    }

    /// <summary>Valida el último código activo del canal. Devuelve el destino verificado.</summary>
    public virtual async Task<VerificationChallenge> VerifyAsync(Guid userId, VerificationChannel channel, string code)
    {
        var now = Clock.Now;
        var query = (await _repository.GetQueryableAsync())
            .Where(x => x.UserId == userId && x.Channel == channel && x.ConsumedTime == null && x.InvalidatedTime == null)
            .OrderByDescending(x => x.CreationTime);
        var challenge = await AsyncExecuter.FirstOrDefaultAsync(query);

        if (challenge is null)
        {
            throw new BusinessException(AccountsErrorCodes.VerificationCodeExpired);
        }

        var result = challenge.Verify(code, now);
        await _repository.UpdateAsync(challenge, autoSave: true);

        return result switch
        {
            VerificationResult.Success => challenge,
            VerificationResult.Invalid => throw new BusinessException(AccountsErrorCodes.VerificationCodeInvalid),
            VerificationResult.TooManyAttempts => throw new BusinessException(AccountsErrorCodes.VerificationTooManyAttempts),
            _ => throw new BusinessException(AccountsErrorCodes.VerificationCodeExpired),
        };
    }

    private static string GenerateNumericCode(int length)
    {
        var max = (int)Math.Pow(10, length);
        return RandomNumberGenerator.GetInt32(0, max).ToString(new string('0', length));
    }
}
