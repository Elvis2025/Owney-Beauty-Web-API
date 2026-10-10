using Ieh.OwneyBeauty.Modules.Accounts.Enums;
namespace Ieh.OwneyBeauty.Modules.Accounts.MyAccount.Dto;

public class UpdateConsentDto
{
    public ConsentType Type { get; set; }
    public bool Granted { get; set; }
}
