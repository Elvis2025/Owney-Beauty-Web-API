using Ieh.OwneyBeauty.Common;
using Ieh.OwneyBeauty.Localization;
using Ieh.OwneyBeauty.Modules.Accounts;
using Ieh.OwneyBeauty.Modules.Platform;
using Volo.Abp.AuditLogging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Validation.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.OpenIddict;
using Volo.Abp.BlobStoring.Database;
using Volo.Abp.TenantManagement;

namespace Ieh.OwneyBeauty;

[DependsOn(
    typeof(AbpAuditLoggingDomainSharedModule),
    typeof(AbpBackgroundJobsDomainSharedModule),
    typeof(AbpFeatureManagementDomainSharedModule),
    typeof(AbpPermissionManagementDomainSharedModule),
    typeof(AbpSettingManagementDomainSharedModule),
    typeof(AbpIdentityDomainSharedModule),
    typeof(AbpOpenIddictDomainSharedModule),
    typeof(AbpTenantManagementDomainSharedModule),
    typeof(BlobStoringDatabaseDomainSharedModule)
    )]
public class OwneyBeautyDomainSharedModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        OwneyBeautyGlobalFeatureConfigurator.Configure();
        OwneyBeautyModuleExtensionConfigurator.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<OwneyBeautyDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<OwneyBeautyResource>("es")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/Localization/OwneyBeauty");

            options.DefaultResourceType = typeof(OwneyBeautyResource);
            
            // Owney Beauty opera en español (por defecto) e inglés.
            options.Languages.Add(new LanguageInfo("es", "es", "Español"));
            options.Languages.Add(new LanguageInfo("en", "en", "English"));
        });
        
        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("OwneyBeauty", typeof(OwneyBeautyResource));
            // Códigos de error de los módulos: Owney.<Módulo>:<Código> (ver Common/OwneyErrorCodes).
            options.MapCodeNamespace(OwneyErrorCodes.Shared.Namespace, typeof(OwneyBeautyResource));
            options.MapCodeNamespace(PlatformErrorCodes.Namespace, typeof(OwneyBeautyResource));
            options.MapCodeNamespace(AccountsErrorCodes.Namespace, typeof(OwneyBeautyResource));
        });
    }
}
