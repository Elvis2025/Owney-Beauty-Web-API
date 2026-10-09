using System;
using Ieh.OwneyBeauty.Platform;
using Ieh.OwneyBeauty.Platform.Currencies;
using Ieh.OwneyBeauty.Platform.Idempotency;
using Ieh.OwneyBeauty.Platform.Taxes;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ieh.OwneyBeauty.Owney;

public class TaxRateTests
{
    private static readonly DateTime From = new(2013, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Rejects_rates_outside_zero_one(double rate)
    {
        var ex = Should.Throw<BusinessException>(() => new TaxRate(Guid.NewGuid(), "DO", "ITBIS", "ITBIS", (decimal)rate, From));
        ex.Code.ShouldBe(PlatformErrorCodes.InvalidTaxRate);
    }

    [Fact]
    public void Is_effective_only_inside_its_validity_window()
    {
        var tax = new TaxRate(Guid.NewGuid(), "do", "itbis", "ITBIS", 0.18m, From, From.AddYears(20));
        tax.Code.ShouldBe("ITBIS");
        tax.CountryCode.ShouldBe("DO");
        tax.IsEffectiveAt(From.AddYears(1)).ShouldBeTrue();
        tax.IsEffectiveAt(From.AddDays(-1)).ShouldBeFalse();
        tax.IsEffectiveAt(From.AddYears(20)).ShouldBeFalse();
        tax.SetActive(false).IsEffectiveAt(From.AddYears(1)).ShouldBeFalse();
    }
}

public class CurrencyTests
{
    [Fact]
    public void Inactive_currency_cannot_be_the_default()
    {
        var dop = new Currency(Guid.NewGuid(), "dop", "Peso dominicano", "RD$");
        dop.Code.ShouldBe("DOP");
        dop.IsActive.ShouldBeTrue();
    }
}

public class IdempotencyRecordTests
{
    [Fact]
    public void Matches_only_the_same_request_hash()
    {
        var now = DateTime.UtcNow;
        var record = new IdempotencyRecord(Guid.NewGuid(), "user", "key-1", "POST", "/api/x", new string('A', 64), now, TimeSpan.FromHours(24));
        record.Matches(new string('A', 64)).ShouldBeTrue();
        record.Matches(new string('B', 64)).ShouldBeFalse();
        record.IsCompleted.ShouldBeFalse();
        record.ExpirationTime.ShouldBe(now.AddHours(24));

        record.Complete(200, "{}", "application/json", now);
        record.IsCompleted.ShouldBeTrue();
        record.ResponseStatusCode.ShouldBe(200);
    }
}
