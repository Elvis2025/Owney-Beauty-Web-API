using Ieh.OwneyBeauty.Books;
using Xunit;

namespace Ieh.OwneyBeauty.EntityFrameworkCore.Applications.Books;

[Collection(OwneyBeautyTestConsts.CollectionDefinitionName)]
public class EfCoreBookAppService_Tests : BookAppService_Tests<OwneyBeautyEntityFrameworkCoreTestModule>
{

}