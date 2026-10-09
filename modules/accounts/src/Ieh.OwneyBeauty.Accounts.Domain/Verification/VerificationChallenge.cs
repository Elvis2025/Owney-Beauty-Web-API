using System;
using System.Security.Cryptography;
using System.Text;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;

namespace Ieh.OwneyBeauty.Accounts.Verification;

/// <summary>
/// Código de un solo uso para verificar correo o teléfono (OB-01-002/003).
/// El código nunca se guarda en claro: solo su SHA-256 con el Id como sal.
/// </summary>
public class VerificationChallenge : AggregateRoot<Guid>, IHasCreationTime
{
    public Guid UserId { get; private set; }

    public VerificationChannel Channel { get; private set; }

    /// <summary>Correo o teléfono normalizado al que se envió el código.</summary>
    public string Target { get; private set; } = null!;

    public string CodeHash { get; private set; } = null!;

    public int FailedAttempts { get; private set; }

    public int MaxAttempts { get; private set; }

    public DateTime CreationTime { get; private set; }

    public DateTime ExpirationTime { get; private set; }

    public DateTime? ConsumedTime { get; private set; }

    public DateTime? InvalidatedTime { get; private set; }

    protected VerificationChallenge()
    {
    }

    public VerificationChallenge(Guid id, Guid userId, VerificationChannel channel, string target, string code, DateTime now, TimeSpan lifetime, int maxAttempts)
        : base(id)
    {
        UserId = userId;
        Channel = channel;
        Target = Check.NotNullOrWhiteSpace(target, nameof(target), AccountsConsts.MaxVerificationTargetLength);
        CodeHash = Hash(id, Check.NotNullOrWhiteSpace(code, nameof(code)));
        CreationTime = now;
        ExpirationTime = now.Add(lifetime);
        MaxAttempts = maxAttempts;
    }

    public bool IsActiveAt(DateTime now) =>
        ConsumedTime is null && InvalidatedTime is null && now < ExpirationTime && FailedAttempts < MaxAttempts;

    /// <summary>
    /// Valida el código. Lanza una excepción de negocio si no es válido; registra el intento fallido.
    /// El llamador debe persistir la entidad incluso cuando falla (para contar intentos).
    /// </summary>
    public VerificationResult Verify(string code, DateTime now)
    {
        if (ConsumedTime is not null || InvalidatedTime is not null || now >= ExpirationTime)
        {
            return VerificationResult.Expired;
        }

        if (FailedAttempts >= MaxAttempts)
        {
            return VerificationResult.TooManyAttempts;
        }

        var expected = Convert.FromHexString(CodeHash);
        var actual = Convert.FromHexString(Hash(Id, code ?? string.Empty));
        if (!CryptographicOperations.FixedTimeEquals(expected, actual))
        {
            FailedAttempts++;
            return FailedAttempts >= MaxAttempts ? VerificationResult.TooManyAttempts : VerificationResult.Invalid;
        }

        ConsumedTime = now;
        return VerificationResult.Success;
    }

    public void Invalidate(DateTime now)
    {
        InvalidatedTime ??= now;
    }

    private static string Hash(Guid id, string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{id:N}:{code.Trim()}"));
        return Convert.ToHexString(bytes);
    }
}

public enum VerificationResult
{
    Success,
    Invalid,
    Expired,
    TooManyAttempts,
}
