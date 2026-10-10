using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Uow;
using Ieh.OwneyBeauty.Modules.Platform.Managers;
using Ieh.OwneyBeauty.Modules.Platform.Models;

namespace Ieh.OwneyBeauty.Modules.Platform.Data;

/// <summary>
/// Datos maestros iniciales de República Dominicana. Idempotente: solo inserta lo que falta.
/// Provincias: códigos ONE (32, incluido el Distrito Nacional).
/// Municipios: solo los de las zonas de lanzamiento. El listado oficial completo debe importarse
/// desde la ONE antes de abrir otras provincias (PLT-M01 queda "En progreso" hasta entonces).
/// </summary>
public class PlatformDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private static readonly (string Code, string Name)[] Provinces =
    {
        ("01", "Distrito Nacional"), ("02", "Azua"), ("03", "Baoruco"), ("04", "Barahona"),
        ("05", "Dajabón"), ("06", "Duarte"), ("07", "Elías Piña"), ("08", "El Seibo"),
        ("09", "Espaillat"), ("10", "Independencia"), ("11", "La Altagracia"), ("12", "La Romana"),
        ("13", "La Vega"), ("14", "María Trinidad Sánchez"), ("15", "Monte Cristi"), ("16", "Pedernales"),
        ("17", "Peravia"), ("18", "Puerto Plata"), ("19", "Hermanas Mirabal"), ("20", "Samaná"),
        ("21", "San Cristóbal"), ("22", "San Juan"), ("23", "San Pedro de Macorís"), ("24", "Sánchez Ramírez"),
        ("25", "Santiago"), ("26", "Santiago Rodríguez"), ("27", "Valverde"), ("28", "Monseñor Nouel"),
        ("29", "Monte Plata"), ("30", "Hato Mayor"), ("31", "San José de Ocoa"), ("32", "Santo Domingo"),
    };

    private static readonly Dictionary<string, string[]> LaunchMunicipalities = new()
    {
        ["01"] = new[] { "Santo Domingo de Guzmán" },
        ["32"] = new[] { "Santo Domingo Este", "Santo Domingo Oeste", "Santo Domingo Norte", "Boca Chica", "Los Alcarrizos", "Pedro Brand", "San Antonio de Guerra" },
        ["25"] = new[] { "Santiago de los Caballeros", "Bisonó", "Jánico", "Licey al Medio", "Puñal", "Sabana Iglesia", "San José de las Matas", "Tamboril", "Villa González" },
        ["12"] = new[] { "La Romana", "Guaymate", "Villa Hermosa" },
        ["23"] = new[] { "San Pedro de Macorís", "Consuelo", "Guayacanes", "Quisqueya", "Ramón Santana", "San José de los Llanos" },
        ["21"] = new[] { "San Cristóbal", "Bajos de Haina", "Cambita Garabitos", "Los Cacaos", "Sabana Grande de Palenque", "San Gregorio de Nigua", "Villa Altagracia", "Yaguate" },
        ["13"] = new[] { "La Vega", "Constanza", "Jarabacoa", "Jima Abajo" },
    };

    private readonly IRepository<Province, Guid> _provinceRepository;
    private readonly IRepository<Municipality, Guid> _municipalityRepository;
    private readonly IRepository<Currency, Guid> _currencyRepository;
    private readonly IRepository<TaxRate, Guid> _taxRateRepository;
    private readonly CurrencyManager _currencyManager;
    private readonly IGuidGenerator _guidGenerator;

    public PlatformDataSeedContributor(
        IRepository<Province, Guid> provinceRepository,
        IRepository<Municipality, Guid> municipalityRepository,
        IRepository<Currency, Guid> currencyRepository,
        IRepository<TaxRate, Guid> taxRateRepository,
        CurrencyManager currencyManager,
        IGuidGenerator guidGenerator)
    {
        _provinceRepository = provinceRepository;
        _municipalityRepository = municipalityRepository;
        _currencyRepository = currencyRepository;
        _taxRateRepository = taxRateRepository;
        _currencyManager = currencyManager;
        _guidGenerator = guidGenerator;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        await SeedGeographyAsync();
        await SeedCurrenciesAsync();
        await SeedTaxRatesAsync();
    }

    private async Task SeedGeographyAsync()
    {
        var existing = (await _provinceRepository.GetListAsync()).ToDictionary(x => x.Code);
        foreach (var (code, name) in Provinces)
        {
            if (!existing.ContainsKey(code))
            {
                existing[code] = await _provinceRepository.InsertAsync(new Province(_guidGenerator.Create(), code, name), autoSave: true);
            }
        }

        var existingMunicipalities = (await _municipalityRepository.GetListAsync()).Select(x => x.Code).ToHashSet();
        foreach (var (provinceCode, names) in LaunchMunicipalities)
        {
            var province = existing[provinceCode];
            for (var i = 0; i < names.Length; i++)
            {
                var code = $"{provinceCode}-{i + 1:00}";
                if (!existingMunicipalities.Contains(code))
                {
                    await _municipalityRepository.InsertAsync(new Municipality(_guidGenerator.Create(), province.Id, code, names[i]), autoSave: true);
                }
            }
        }
    }

    private async Task SeedCurrenciesAsync()
    {
        if (await _currencyRepository.GetCountAsync() > 0)
        {
            return;
        }

        var dop = await _currencyManager.CreateAsync("DOP", "Peso dominicano", "RD$", 2);
        await _currencyManager.SetDefaultAsync(dop);
        await _currencyRepository.InsertAsync(dop, autoSave: true);
        await _currencyRepository.InsertAsync(await _currencyManager.CreateAsync("USD", "Dólar estadounidense", "US$", 2), autoSave: true);
    }

    private async Task SeedTaxRatesAsync()
    {
        if (await _taxRateRepository.AnyAsync(x => x.Code == "ITBIS" && x.CountryCode == "DO"))
        {
            return;
        }

        await _taxRateRepository.InsertAsync(new TaxRate(
            _guidGenerator.Create(), "DO", "ITBIS", "ITBIS (tasa general)", 0.18m,
            new DateTime(2013, 1, 1, 0, 0, 0, DateTimeKind.Utc)), autoSave: true);
    }
}
