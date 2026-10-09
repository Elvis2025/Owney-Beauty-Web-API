using Ieh.OwneyBeauty.Accounts.Legal;
using Ieh.OwneyBeauty.Accounts.Profiles;
using Ieh.OwneyBeauty.Accounts.Verification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.Modularity;

namespace Ieh.OwneyBeauty.Accounts.EntityFrameworkCore;

[ConnectionStringName(AccountsDbProperties.ConnectionStringName)]
public interface IAccountsDbContext : IEfCoreDbContext
{
    DbSet<AccountProfile> AccountProfiles { get; }
    DbSet<VerificationChallenge> VerificationChallenges { get; }
    DbSet<LegalDocument> LegalDocuments { get; }
    DbSet<ConsentRecord> ConsentRecords { get; }
}

[ConnectionStringName(AccountsDbProperties.ConnectionStringName)]
public class AccountsDbContext : AbpDbContext<AccountsDbContext>, IAccountsDbContext
{
    public DbSet<AccountProfile> AccountProfiles { get; set; } = null!;
    public DbSet<VerificationChallenge> VerificationChallenges { get; set; } = null!;
    public DbSet<LegalDocument> LegalDocuments { get; set; } = null!;
    public DbSet<ConsentRecord> ConsentRecords { get; set; } = null!;

    public AccountsDbContext(DbContextOptions<AccountsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureAccounts();
    }
}

public static class AccountsDbContextModelCreatingExtensions
{
    public static void ConfigureAccounts(this ModelBuilder builder)
    {
        Volo.Abp.Check.NotNull(builder, nameof(builder));
        const string schema = AccountsDbProperties.DbSchema;
        const string prefix = AccountsDbProperties.DbTablePrefix;

        builder.Entity<AccountProfile>(b =>
        {
            b.ToTable(prefix + "AccountProfiles", schema);
            b.ConfigureByConvention();
            b.Property(x => x.Id).ValueGeneratedNever();
        });

        builder.Entity<VerificationChallenge>(b =>
        {
            b.ToTable(prefix + "VerificationChallenges", schema);
            b.ConfigureByConvention();
            b.Property(x => x.Target).IsRequired().HasMaxLength(AccountsConsts.MaxVerificationTargetLength);
            b.Property(x => x.CodeHash).IsRequired().HasMaxLength(AccountsConsts.VerificationCodeHashLength);
            b.HasIndex(x => new { x.UserId, x.Channel, x.CreationTime });
        });

        builder.Entity<LegalDocument>(b =>
        {
            b.ToTable(prefix + "LegalDocuments", schema);
            b.ConfigureByConvention();
            b.Property(x => x.Language).IsRequired().HasMaxLength(AccountsConsts.MaxLanguageLength);
            b.Property(x => x.Version).IsRequired().HasMaxLength(AccountsConsts.MaxLegalVersionLength);
            b.Property(x => x.Title).IsRequired().HasMaxLength(AccountsConsts.MaxLegalTitleLength);
            b.Property(x => x.Content).HasMaxLength(AccountsConsts.MaxLegalContentLength);
            b.Property(x => x.ExternalUrl).HasMaxLength(AccountsConsts.MaxLegalUrlLength);
            b.HasIndex(x => new { x.Type, x.Language, x.Version }).IsUnique();
            b.HasIndex(x => new { x.Type, x.Language, x.IsCurrent });
        });

        builder.Entity<ConsentRecord>(b =>
        {
            b.ToTable(prefix + "ConsentRecords", schema);
            b.ConfigureByConvention();
            b.Property(x => x.LegalDocumentVersion).HasMaxLength(AccountsConsts.MaxLegalVersionLength);
            b.Property(x => x.ClientIpAddress).HasMaxLength(AccountsConsts.MaxClientIpLength);
            b.HasIndex(x => new { x.UserId, x.CreationTime });
            b.HasIndex(x => x.LegalDocumentId);
        });
    }
}

[DependsOn(
    typeof(AccountsDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class AccountsEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<AccountsDbContext>(options =>
        {
            options.AddDefaultRepositories<IAccountsDbContext>(includeAllEntities: true);
        });
    }
}
