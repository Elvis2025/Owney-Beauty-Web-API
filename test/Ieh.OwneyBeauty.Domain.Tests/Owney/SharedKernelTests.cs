using System;
using Shouldly;
using Volo.Abp;
using Xunit;
using Ieh.OwneyBeauty.Common;

namespace Ieh.OwneyBeauty.Owney;

public class MoneyTests
{
    [Fact]
    public void Rounds_to_two_decimals_and_normalizes_currency()
    {
        var money = new Money(10.005m, "dop");
        money.Amount.ShouldBe(10.00m);
        money.Currency.ShouldBe("DOP");
    }

    [Fact]
    public void Adds_and_subtracts_same_currency()
    {
        (new Money(100m) + new Money(18m)).ValueEquals(new Money(118m)).ShouldBeTrue();
        (new Money(100m) - new Money(25.5m)).Amount.ShouldBe(74.5m);
    }

    [Fact]
    public void Rejects_mixing_currencies()
    {
        var ex = Should.Throw<BusinessException>(() => new Money(1m, "DOP").Add(new Money(1m, "USD")));
        ex.Code.ShouldBe(OwneyErrorCodes.Shared.CurrencyMismatch);
    }

    [Fact]
    public void Applies_tax_rate_as_fraction()
    {
        new Money(1000m).Percentage(0.18m).Amount.ShouldBe(180m);
    }
}

public class GeoPointTests
{
    [Fact]
    public void Rejects_out_of_range_coordinates()
    {
        Should.Throw<BusinessException>(() => new GeoPoint(91, 0));
        Should.Throw<BusinessException>(() => new GeoPoint(0, -181));
    }

    [Fact]
    public void Computes_haversine_distance()
    {
        // Santo Domingo (Zona Colonial) → Santiago de los Caballeros ≈ 125 km en línea recta.
        var santoDomingo = new GeoPoint(18.4728, -69.8846);
        var santiago = new GeoPoint(19.4517, -70.6970);
        santoDomingo.DistanceKmTo(santiago).ShouldBeInRange(120, 140);
    }

    [Fact]
    public void Approximates_location_for_public_display()
    {
        new GeoPoint(18.47281234, -69.88461234).Approximate().ValueEquals(new GeoPoint(18.47, -69.88)).ShouldBeTrue();
    }
}

public class DateTimeRangeTests
{
    private static DateTime Utc(int hour) => new(2026, 10, 9, hour, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Requires_utc_and_ordered_bounds()
    {
        Should.Throw<BusinessException>(() => new DateTimeRange(DateTime.Now, DateTime.Now.AddHours(1)));
        Should.Throw<BusinessException>(() => new DateTimeRange(Utc(10), Utc(9)));
    }

    [Fact]
    public void Detects_overlaps_with_half_open_semantics()
    {
        var morning = new DateTimeRange(Utc(9), Utc(10));
        morning.Overlaps(new DateTimeRange(Utc(9), Utc(11))).ShouldBeTrue();
        morning.Overlaps(new DateTimeRange(Utc(10), Utc(11))).ShouldBeFalse(); // consecutivas no se solapan
        morning.Contains(Utc(10)).ShouldBeFalse();
    }
}
