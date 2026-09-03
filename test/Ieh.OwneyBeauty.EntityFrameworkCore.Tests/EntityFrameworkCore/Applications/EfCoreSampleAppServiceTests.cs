using Ieh.OwneyBeauty.Samples;
using Xunit;

namespace Ieh.OwneyBeauty.EntityFrameworkCore.Applications;

[Collection(OwneyBeautyTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<OwneyBeautyEntityFrameworkCoreTestModule>
{

}
