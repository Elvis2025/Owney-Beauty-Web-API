using System;
using Ieh.OwneyBeauty.Modules.Accounts.Enums;

namespace Ieh.OwneyBeauty.Modules.Accounts.Verification.Dto;

public class VerificationCodeSentDto
{
    public VerificationChannel Channel { get; set; }

    /// <summary>Destino enmascarado, p. ej. "ma***@gmail.com" o "+1809*****34".</summary>
    public string MaskedTarget { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }
}
