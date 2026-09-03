using System.Threading.Tasks;

namespace Ieh.OwneyBeauty.Data;

public interface IOwneyBeautyDbSchemaMigrator
{
    Task MigrateAsync();
}
