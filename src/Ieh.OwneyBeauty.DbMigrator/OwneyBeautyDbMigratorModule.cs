using Ieh.OwneyBeauty.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Ieh.OwneyBeauty.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(OwneyBeautyEntityFrameworkCoreModule),
    typeof(OwneyBeautyApplicationContractsModule)
)]
public class OwneyBeautyDbMigratorModule : AbpModule
{
}
