using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Ieh.OwneyBeauty.Data;

/* This is used if database provider does't define
 * IOwneyBeautyDbSchemaMigrator implementation.
 */
public class NullOwneyBeautyDbSchemaMigrator : IOwneyBeautyDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
