using Ieh.OwneyBeauty.Accounts.EntityFrameworkCore;
using Ieh.OwneyBeauty.Accounts.Legal;
using Ieh.OwneyBeauty.Accounts.Profiles;
using Ieh.OwneyBeauty.Accounts.Verification;
using Ieh.OwneyBeauty.Platform.Auditing;
using Ieh.OwneyBeauty.Platform.Currencies;
using Ieh.OwneyBeauty.Platform.EntityFrameworkCore;
using Ieh.OwneyBeauty.Platform.Geography;
using Ieh.OwneyBeauty.Platform.Idempotency;
using Ieh.OwneyBeauty.Platform.Taxes;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DistributedEvents;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace Ieh.OwneyBeauty.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ReplaceDbContext(typeof(IPlatformDbContext))]
[ReplaceDbContext(typeof(IAccountsDbContext))]
[ConnectionStringName("Default")]
public class OwneyBeautyDbContext :
    AbpDbContext<OwneyBeautyDbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext,
    IPlatformDbContext,
    IAccountsDbContext,
    IHasEventOutbox,
    IHasEventInbox
{
    /* Los módulos de negocio definen sus entidades y configuración; aquí solo se componen
     * para que todo el monolito comparta transacción, migraciones y outbox. */

    // Platform
    public DbSet<Province> Provinces { get; set; }
    public DbSet<Municipality> Municipalities { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<TaxRate> TaxRates { get; set; }
    public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }
    public DbSet<BusinessAuditEvent> BusinessAuditEvents { get; set; }

    // Accounts
    public DbSet<AccountProfile> AccountProfiles { get; set; }
    public DbSet<VerificationChallenge> VerificationChallenges { get; set; }
    public DbSet<LegalDocument> LegalDocuments { get; set; }
    public DbSet<ConsentRecord> ConsentRecords { get; set; }

    // Eventos distribuidos (outbox/inbox)
    public DbSet<OutgoingEventRecord> OutgoingEvents { get; set; }
    public DbSet<IncomingEventRecord> IncomingEvents { get; set; }

    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext and ISaasDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext and ISaasDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public OwneyBeautyDbContext(DbContextOptions<OwneyBeautyDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();
        builder.ConfigureEventOutbox();
        builder.ConfigureEventInbox();

        /* Módulos de Owney (cada uno en su esquema) */
        builder.ConfigurePlatform();
        builder.ConfigureAccounts();

        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(OwneyBeautyConsts.DbTablePrefix + "YourEntities", OwneyBeautyConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});
    }
}
