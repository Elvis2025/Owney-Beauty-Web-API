using System.ComponentModel.DataAnnotations;

namespace Ieh.OwneyBeauty.Modules.Accounts.Verification.Dto;

public class ConfirmCodeDto
{
    [Required, StringLength(AccountsConsts.VerificationCodeLength, MinimumLength = AccountsConsts.VerificationCodeLength)]
    [RegularExpression(@"^\d+$")]
    public string Code { get; set; } = null!;
}
