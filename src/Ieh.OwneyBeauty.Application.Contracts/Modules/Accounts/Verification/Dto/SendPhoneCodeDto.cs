using System.ComponentModel.DataAnnotations;

namespace Ieh.OwneyBeauty.Modules.Accounts.Verification.Dto;

public class SendPhoneCodeDto
{
    /// <summary>Si se indica, reemplaza el teléfono actual (pendiente de verificación).</summary>
    [RegularExpression(AccountsConsts.PhoneRegex), StringLength(AccountsConsts.MaxPhoneLength)]
    public string? PhoneNumber { get; set; }
}
