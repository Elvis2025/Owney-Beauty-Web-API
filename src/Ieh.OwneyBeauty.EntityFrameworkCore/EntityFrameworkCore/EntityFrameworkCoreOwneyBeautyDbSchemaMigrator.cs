using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ieh.OwneyBeauty.Data;
using Volo.Abp.DependencyInjection;

namespace Ieh.OwneyBeauty.EntityFrameworkCore;

public class EntityFrameworkCoreOwneyBeautyDbSchemaMigrator
    : IOwneyBeautyDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreOwneyBeautyDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the OwneyBeautyDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<OwneyBeautyDbContext>()
            .Database
            .MigrateAsync();
    }
}
