using Ieh.OwneyBeauty.Platform.Auditing;
using Ieh.OwneyBeauty.Platform.Currencies;
using Ieh.OwneyBeauty.Platform.Geography;
using Ieh.OwneyBeauty.Platform.Idempotency;
using Ieh.OwneyBeauty.Platform.Taxes;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Ieh.OwneyBeauty.Platform.EntityFrameworkCore;

[ConnectionStringName(PlatformDbProperties.ConnectionStringName)]
public interface IPlatformDbContext : IEfCoreDbContext
{
    DbSet<Province> Provinces { get; }
    DbSet<Municipality> Municipalities { get; }
    DbSet<Currency> Currencies { get; }
    DbSet<TaxRate> TaxRates { get; }
    DbSet<IdempotencyRecord> IdempotencyRecords { get; }
    DbSet<BusinessAuditEvent> BusinessAuditEvents { get; }
}

/// <summary>
/// DbContext propio del módulo. En el monolito, el DbContext del host lo reemplaza
/// (<c>[ReplaceDbContext(typeof(IPlatformDbContext))]</c>) para compartir transacción y outbox.
/// Si el módulo se extrae como servicio, basta con quitar ese reemplazo.
/// </summary>
[ConnectionStringName(PlatformDbProperties.ConnectionStringName)]
public class PlatformDbContext : AbpDbContext<PlatformDbContext>, IPlatformDbContext
{
    public DbSet<Province> Provinces { get; set; } = null!;
    public DbSet<Municipality> Municipalities { get; set; } = null!;
    public DbSet<Currency> Currencies { get; set; } = null!;
    public DbSet<TaxRate> TaxRates { get; set; } = null!;
    public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; } = null!;
    public DbSet<BusinessAuditEvent> BusinessAuditEvents { get; set; } = null!;

    public PlatformDbContext(DbContextOptions<PlatformDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigurePlatform();
    }
}

public static class PlatformDbContextModelCreatingExtensions
{
    public static void ConfigurePlatform(this ModelBuilder builder)
    {
        Volo.Abp.Check.NotNull(builder, nameof(builder));
        const string schema = PlatformDbProperties.DbSchema;
        const string prefix = PlatformDbProperties.DbTablePrefix;

        builder.Entity<Province>(b =>
        {
            b.ToTable(prefix + "Provinces", schema);
            b.ConfigureByConvention();
            b.Property(x => x.CountryCode).IsRequired().HasMaxLength(PlatformConsts.CountryCodeLength);
            b.Property(x => x.Code).IsRequired().HasMaxLength(PlatformConsts.MaxCodeLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(PlatformConsts.MaxNameLength);
            b.HasIndex(x => new { x.CountryCode, x.Code }).IsUnique();
        });

        builder.Entity<Municipality>(b =>
        {
            b.ToTable(prefix + "Municipalities", schema);
            b.ConfigureByConvention();
            b.Property(x => x.Code).IsRequired().HasMaxLength(PlatformConsts.MaxCodeLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(PlatformConsts.MaxNameLength);
            b.HasOne<Province>().WithMany().HasForeignKey(x => x.ProvinceId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.Code).IsUnique();
            b.HasIndex(x => new { x.ProvinceId, x.Name }).IsUnique();
        });

        builder.Entity<Currency>(b =>
        {
            b.ToTable(prefix + "Currencies", schema);
            b.ConfigureByConvention();
            b.Property(x => x.Code).IsRequired().HasMaxLength(PlatformConsts.CurrencyCodeLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(PlatformConsts.MaxNameLength);
            b.Property(x => x.Symbol).IsRequired().HasMaxLength(PlatformConsts.MaxCurrencySymbolLength);
            b.HasIndex(x => x.Code).IsUnique();
        });

        builder.Entity<TaxRate>(b =>
        {
            b.ToTable(prefix + "TaxRates", schema);
            b.ConfigureByConvention();
            b.Property(x => x.CountryCode).IsRequired().HasMaxLength(PlatformConsts.CountryCodeLength);
            b.Property(x => x.Code).IsRequired().HasMaxLength(PlatformConsts.MaxCodeLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(PlatformConsts.MaxNameLength);
            b.Property(x => x.Rate).HasPrecision(7, 6);
            b.HasIndex(x => new { x.CountryCode, x.Code, x.EffectiveFrom }).IsUnique();
        });

        builder.Entity<IdempotencyRecord>(b =>
        {
            b.ToTable(prefix + "IdempotencyRecords", schema);
            b.ConfigureByConvention();
            b.Property(x => x.Scope).IsRequired().HasMaxLength(PlatformConsts.MaxIdempotencyScopeLength);
            b.Property(x => x.Key).IsRequired().HasMaxLength(PlatformConsts.MaxIdempotencyKeyLength);
            b.Property(x => x.Method).IsRequired().HasMaxLength(16);
            b.Property(x => x.Path).IsRequired().HasMaxLength(PlatformConsts.MaxRequestPathLength);
            b.Property(x => x.RequestHash).IsRequired().HasMaxLength(PlatformConsts.RequestHashLength);
            b.Property(x => x.ResponseContentType).HasMaxLength(PlatformConsts.MaxResponseContentTypeLength);
            b.HasIndex(x => new { x.Scope, x.Key }).IsUnique();
            b.HasIndex(x => x.ExpirationTime);
        });

        builder.Entity<BusinessAuditEvent>(b =>
        {
            b.ToTable(prefix + "BusinessAuditEvents", schema);
            b.ConfigureByConvention();
            b.Property(x => x.Module).IsRequired().HasMaxLength(PlatformConsts.MaxAuditModuleLength);
            b.Property(x => x.Action).IsRequired().HasMaxLength(PlatformConsts.MaxAuditActionLength);
            b.Property(x => x.EntityType).IsRequired().HasMaxLength(PlatformConsts.MaxAuditEntityTypeLength);
            b.Property(x => x.EntityId).HasMaxLength(PlatformConsts.MaxAuditEntityIdLength);
            b.Property(x => x.Reason).HasMaxLength(PlatformConsts.MaxAuditReasonLength);
            b.Property(x => x.UserName).HasMaxLength(PlatformConsts.MaxAuditUserNameLength);
            b.Property(x => x.ClientIpAddress).HasMaxLength(PlatformConsts.MaxAuditClientIpLength);
            b.Property(x => x.CorrelationId).HasMaxLength(PlatformConsts.MaxAuditCorrelationIdLength);
            b.HasIndex(x => x.CreationTime);
            b.HasIndex(x => new { x.Module, x.Action });
            b.HasIndex(x => new { x.EntityType, x.EntityId });
            b.HasIndex(x => x.UserId);
        });
    }
}
