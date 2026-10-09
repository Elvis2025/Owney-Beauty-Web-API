using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace Ieh.OwneyBeauty.ArchitectureTests;

/// <summary>
/// PLT-F04 — Reglas del monolito modular (§2.2 del plan). Descubre automáticamente todos los módulos
/// (modules/*), así que los módulos nuevos quedan cubiertos sin tocar este archivo.
/// Para que un módulo nuevo se verifique, añada sus proyectos como referencia de este proyecto de pruebas.
/// </summary>
public class ModuleBoundaryTests
{
    private static readonly Regex ModuleAssembly = new(
        @"^Ieh\.OwneyBeauty\.(?<module>[A-Z][A-Za-z]+)\.(?<layer>Domain\.Shared|Domain|Application\.Contracts|Application|EntityFrameworkCore)$",
        RegexOptions.Compiled);

    private static readonly HashSet<string> HostNames = new(StringComparer.Ordinal)
    {
        "Domain", "Application", "EntityFrameworkCore", "HttpApi", "SharedKernel", "Infrastructure", "Worker", "DbMigrator",
    };

    private static readonly string[] HostAssemblies =
    {
        "Ieh.OwneyBeauty.Domain", "Ieh.OwneyBeauty.Domain.Shared", "Ieh.OwneyBeauty.Application",
        "Ieh.OwneyBeauty.Application.Contracts", "Ieh.OwneyBeauty.EntityFrameworkCore", "Ieh.OwneyBeauty.HttpApi",
        "Ieh.OwneyBeauty.HttpApi.Host", "Ieh.OwneyBeauty.Infrastructure", "Ieh.OwneyBeauty.Worker",
    };

    public static IEnumerable<object[]> Modules() => LoadModuleAssemblies().Select(m => new object[] { m.Name });

    [Fact]
    public void At_least_the_known_modules_are_discovered()
    {
        var names = LoadModuleAssemblies().Select(x => x.Module).Distinct().ToList();
        names.ShouldContain("Platform");
        names.ShouldContain("Accounts");
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void Module_does_not_depend_on_the_host(string assemblyName)
    {
        var references = ReferencesOf(assemblyName);
        references.Intersect(HostAssemblies).ShouldBeEmpty($"{assemblyName} no puede depender del host");
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void Module_only_uses_public_contracts_of_other_modules(string assemblyName)
    {
        var (module, layer) = Parse(assemblyName);
        foreach (var reference in ReferencesOf(assemblyName))
        {
            var match = ModuleAssembly.Match(reference);
            if (!match.Success || HostNames.Contains(match.Groups["module"].Value) || match.Groups["module"].Value == module)
            {
                continue;
            }

            var otherLayer = match.Groups["layer"].Value;
            var allowed =
                // Eventos (ETOs), enums y constantes públicas de otro módulo.
                otherLayer == "Domain.Shared" ||
                // Integration services: solo desde la capa Application.
                (otherLayer == "Application.Contracts" && layer == "Application");

            allowed.ShouldBeTrue($"{assemblyName} referencia {reference}: solo se permiten Domain.Shared y (desde Application) Application.Contracts de otros módulos.");
        }
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void Layers_respect_ddd_direction(string assemblyName)
    {
        var (module, layer) = Parse(assemblyName);
        var references = ReferencesOf(assemblyName).ToHashSet();
        string Own(string l) => $"Ieh.OwneyBeauty.{module}.{l}";

        switch (layer)
        {
            case "Domain.Shared":
                references.ShouldNotContain(Own("Domain"));
                references.ShouldNotContain(Own("Application"));
                references.ShouldNotContain(Own("EntityFrameworkCore"));
                break;
            case "Domain":
                references.ShouldNotContain(Own("Application"));
                references.ShouldNotContain(Own("Application.Contracts"));
                references.ShouldNotContain(Own("EntityFrameworkCore"));
                references.ShouldNotContain("Volo.Abp.EntityFrameworkCore");
                break;
            case "Application.Contracts":
                references.ShouldNotContain(Own("Domain"));
                references.ShouldNotContain(Own("EntityFrameworkCore"));
                break;
            case "Application":
                references.ShouldNotContain(Own("EntityFrameworkCore"));
                references.ShouldNotContain("Microsoft.EntityFrameworkCore");
                break;
        }
    }

    [Fact]
    public void Shared_kernel_does_not_depend_on_modules_or_host()
    {
        var references = ReferencesOf("Ieh.OwneyBeauty.SharedKernel");
        references.Where(r => r.StartsWith("Ieh.OwneyBeauty.", StringComparison.Ordinal)).ShouldBeEmpty();
    }

    private static (string Module, string Layer) Parse(string assemblyName)
    {
        var match = ModuleAssembly.Match(assemblyName);
        return (match.Groups["module"].Value, match.Groups["layer"].Value);
    }

    private static IEnumerable<string> ReferencesOf(string assemblyName) =>
        Assembly.Load(assemblyName).GetReferencedAssemblies().Select(a => a.Name!).ToList();

    private static List<(string Name, string Module)> LoadModuleAssemblies()
    {
        var directory = AppContext.BaseDirectory;
        return Directory.GetFiles(directory, "Ieh.OwneyBeauty.*.dll")
            .Select(Path.GetFileNameWithoutExtension)
            .Select(name => (Name: name!, Match: ModuleAssembly.Match(name!)))
            .Where(x => x.Match.Success && !HostNames.Contains(x.Match.Groups["module"].Value))
            .Select(x => (x.Name, x.Match.Groups["module"].Value))
            .OrderBy(x => x.Name)
            .ToList();
    }
}
