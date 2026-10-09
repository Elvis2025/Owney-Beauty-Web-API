using Ieh.OwneyBeauty.Owney;
using Xunit;

namespace Ieh.OwneyBeauty.EntityFrameworkCore.Applications;

[Collection(OwneyBeautyTestConsts.CollectionDefinitionName)]
public class EfCoreAccountsAppServiceTests : AccountsAppServiceTests<OwneyBeautyEntityFrameworkCoreTestModule>
{
}

[Collection(OwneyBeautyTestConsts.CollectionDefinitionName)]
public class EfCorePlatformAppServiceTests : PlatformAppServiceTests<OwneyBeautyEntityFrameworkCoreTestModule>
{
}
