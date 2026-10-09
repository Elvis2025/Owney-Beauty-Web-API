using Volo.Abp.Domain;
using Volo.Abp.Emailing;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.Sms;

namespace Ieh.OwneyBeauty.Accounts;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(AbpIdentityDomainModule),
    typeof(AbpEmailingModule),
    typeof(AbpSmsModule),
    typeof(AccountsDomainSharedModule)
)]
public class AccountsDomainModule : AbpModule
{
}
