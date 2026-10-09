using System;
using System.Linq;
using System.Threading.Tasks;
using Ieh.OwneyBeauty.Platform;
using Ieh.OwneyBeauty.Platform.Currencies;
using Ieh.OwneyBeauty.Platform.Geography;
using Ieh.OwneyBeauty.Platform.Taxes;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Modularity;
using Xunit;

namespace Ieh.OwneyBeauty.Owney;

public abstract class PlatformAppServiceTests<TStartupModule> : OwneyBeautyApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IGeographyAppService _geography;
    private readonly IProvinceAppService _provinces;
    private readonly ICurrencyAppService _currencies;
    private readonly ITaxRateAppService _taxRates;

    protected PlatformAppServiceTests()
    {
        _geography = GetRequiredService<IGeographyAppService>();
        _provinces = GetRequiredService<IProvinceAppService>();
        _currencies = GetRequiredService<ICurrencyAppService>();
        _taxRates = GetRequiredService<ITaxRateAppService>();
    }

    [Fact]
    public async Task Seeds_the_32_provinces_and_launch_municipalities()
    {
        var provinces = await _geography.GetProvincesAsync();
        provinces.Items.Count.ShouldBe(32);

        var santiago = provinces.Items.Single(x => x.Code == "25");
        (await _geography.GetMunicipalitiesAsync(santiago.Id)).Items.ShouldContain(x => x.Name == "Santiago de los Caballeros");
    }

    [Fact]
    public async Task Province_codes_are_unique()
    {
        var ex = await Should.ThrowAsync<BusinessException>(() =>
            _provinces.CreateAsync(new CreateUpdateProvinceDto { Code = "01", Name = "Duplicada" }));
        ex.Code.ShouldBe(PlatformErrorCodes.DuplicateCode);
    }

    [Fact]
    public async Task Only_one_default_currency()
    {
        var all = await _currencies.GetListAsync();
        var usd = all.Items.Single(x => x.Code == "USD");
        await _currencies.SetDefaultAsync(usd.Id);

        var after = await _currencies.GetListAsync();
        after.Items.Count(x => x.IsDefault).ShouldBe(1);
        after.Items.Single(x => x.IsDefault).Code.ShouldBe("USD");

        await _currencies.SetDefaultAsync(after.Items.Single(x => x.Code == "DOP").Id);
    }

    [Fact]
    public async Task Effective_itbis_is_18_percent()
    {
        var itbis = await _taxRates.GetEffectiveAsync("DO", "ITBIS", DateTime.UtcNow);
        itbis.ShouldNotBeNull();
        itbis!.Rate.ShouldBe(0.18m);
    }
}
