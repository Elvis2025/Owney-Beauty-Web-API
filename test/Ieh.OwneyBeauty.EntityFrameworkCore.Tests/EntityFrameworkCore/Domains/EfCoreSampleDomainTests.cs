using Ieh.OwneyBeauty.Samples;
using Xunit;

namespace Ieh.OwneyBeauty.EntityFrameworkCore.Domains;

[Collection(OwneyBeautyTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<OwneyBeautyEntityFrameworkCoreTestModule>
{

}
