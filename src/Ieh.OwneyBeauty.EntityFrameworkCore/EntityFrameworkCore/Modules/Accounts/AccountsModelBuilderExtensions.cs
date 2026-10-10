using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Ieh.OwneyBeauty.Modules.Accounts;
using Ieh.OwneyBeauty.Modules.Accounts.Models;

namespace Ieh.OwneyBeauty.EntityFrameworkCore.Modules.Accounts;

public static class AccountsModelBuilderExtensions
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
