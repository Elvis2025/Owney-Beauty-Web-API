using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Ieh.OwneyBeauty.Modules.Platform;
using Ieh.OwneyBeauty.Modules.Platform.Models;

namespace Ieh.OwneyBeauty.EntityFrameworkCore.Modules.Platform;

public static class PlatformModelBuilderExtensions
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
