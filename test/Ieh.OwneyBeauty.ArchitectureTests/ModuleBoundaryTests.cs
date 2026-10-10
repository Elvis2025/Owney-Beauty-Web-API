using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ieh.OwneyBeauty.EntityFrameworkCore;
using NetArchTest.Rules;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Xunit;

namespace Ieh.OwneyBeauty.ArchitectureTests;

/// <summary>
/// PLT-F04 — Reglas de la arquitectura por módulos. Cada módulo es una carpeta <c>Modules/&lt;Módulo&gt;</c>
/// dentro de cada capa de ABP (Domain.Shared, Domain, Application.Contracts, Application, EntityFrameworkCore).
/// Los módulos se descubren por namespace, así que los módulos nuevos quedan cubiertos sin tocar este archivo.
/// </summary>
public class ModuleBoundaryTests
{
    private const string ModulesNamespace = "Ieh.OwneyBeauty.Modules.";

    private static readonly Assembly DomainShared = typeof(OwneyBeautyDomainSharedModule).Assembly;
    private static readonly Assembly Domain = typeof(OwneyBeautyDomainModule).Assembly;
    private static readonly Assembly ApplicationContracts = typeof(OwneyBeautyApplicationContractsModule).Assembly;
    private static readonly Assembly Application = typeof(OwneyBeautyApplicationModule).Assembly;
    private static readonly Assembly EntityFrameworkCore = typeof(OwneyBeautyEntityFrameworkCoreModule).Assembly;

    private static readonly Assembly[] Layers = { DomainShared, Domain, ApplicationContracts, Application, EntityFrameworkCore };

    /// <summary>
    /// Lo único que un módulo puede usar de otro: constantes de la raíz, enums, eventos (ETOs) e
    /// integration services (lecturas síncronas, <c>Modules/&lt;Módulo&gt;/Integration</c>).
    /// </summary>
    private static readonly string[] PublicSegments = { "", ".Enums", ".Events", ".Integration", ".Integration.Dto" };

    public static IEnumerable<object[]> ModulePairs() =>
        from module in DiscoverModules()
        from other in DiscoverModules()
        where module != other
        select new object[] { module, other };

    public static IEnumerable<object[]> Modules() => DiscoverModules().Select(m => new object[] { m });

    [Fact]
    public void At_least_the_known_modules_are_discovered()
    {
        var modules = DiscoverModules();
        modules.ShouldContain("Platform");
        modules.ShouldContain("Accounts");
    }

    [Theory]
    [MemberData(nameof(ModulePairs))]
    public void Module_only_uses_public_contracts_of_other_modules(string module, string other)
    {
        var otherPublic = PublicSegments.Select(s => ModulesNamespace + other + s).ToHashSet();
        var forbidden = ModuleTypes(other, Layers)
            .Select(t => t.Namespace!)
            .Where(ns => !otherPublic.Contains(ns))
            .Distinct()
            .ToArray();

        if (forbidden.Length == 0)
        {
            return;
        }

        var result = Types.InAssemblies(Layers)
            .That().ResideInNamespaceStartingWith(ModulesNamespace + module + ".")
            .Or().ResideInNamespace(ModulesNamespace + module)
            .ShouldNot().HaveDependencyOnAny(forbidden)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"{module} usa partes internas de {other}: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}. " +
            "Entre módulos solo se comparten constantes, enums, eventos e integration services.");
    }

    [Fact]
    public void Common_does_not_depend_on_modules()
    {
        var result = Types.InAssemblies(Layers)
            .That().ResideInNamespaceStartingWith("Ieh.OwneyBeauty.Common")
            .ShouldNot().HaveDependencyOn("Ieh.OwneyBeauty.Modules")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Domain_and_application_do_not_depend_on_entity_framework()
    {
        foreach (var assembly in new[] { DomainShared, Domain, ApplicationContracts, Application })
        {
            var references = assembly.GetReferencedAssemblies().Select(a => a.Name).ToList();
            references.ShouldNotContain("Microsoft.EntityFrameworkCore", $"{assembly.GetName().Name} no puede depender de EF Core");
            references.ShouldNotContain("Volo.Abp.EntityFrameworkCore", $"{assembly.GetName().Name} no puede depender de EF Core");
        }
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void App_services_live_in_a_Service_folder(string module)
    {
        ModuleTypes(module, ApplicationContracts, Application)
            .Where(t => typeof(IApplicationService).IsAssignableFrom(t))
            .Where(t => !t.Namespace!.EndsWith(".Service", StringComparison.Ordinal) && !t.Namespace.EndsWith(".Integration", StringComparison.Ordinal))
            .Select(t => t.FullName)
            .ShouldBeEmpty("Los app services y sus interfaces van en Modules/<Módulo>/<Funcionalidad>/Service");
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void Dtos_live_in_a_Dto_folder(string module)
    {
        ModuleTypes(module, ApplicationContracts)
            .Where(t => t.IsClass && (t.Name.EndsWith("Dto", StringComparison.Ordinal) || t.Name.EndsWith("Input", StringComparison.Ordinal)
                                      || typeof(IEntityDto).IsAssignableFrom(t)))
            .Where(t => !t.Namespace!.EndsWith(".Dto", StringComparison.Ordinal))
            .Select(t => t.FullName)
            .ShouldBeEmpty("Los DTOs van en Modules/<Módulo>/<Funcionalidad>/Dto");
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void Mappers_live_in_a_Mappers_folder(string module)
    {
        ModuleTypes(module, Application)
            .Where(t => t.Name.EndsWith("Mapper", StringComparison.Ordinal))
            .Where(t => !t.Namespace!.EndsWith(".Mappers", StringComparison.Ordinal))
            .Select(t => t.FullName)
            .ShouldBeEmpty("Los mappers de Mapperly van en Modules/<Módulo>/<Funcionalidad>/Mappers");
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void Entities_live_in_the_Models_folder(string module)
    {
        ModuleTypes(module, Domain)
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IEntity).IsAssignableFrom(t))
            .Where(t => t.Namespace != $"{ModulesNamespace}{module}.Models")
            .Select(t => t.FullName)
            .ShouldBeEmpty("Las entidades van en Modules/<Módulo>/Models");
    }

    [Fact]
    public void Each_layer_only_contains_the_folders_it_owns()
    {
        // DTOs y mappers nunca en Domain, Domain.Shared ni EntityFrameworkCore.
        ModuleTypes(null, Domain, DomainShared, EntityFrameworkCore)
            .Where(t => t.Namespace!.EndsWith(".Dto", StringComparison.Ordinal) || t.Namespace.EndsWith(".Mappers", StringComparison.Ordinal))
            .Select(t => t.FullName)
            .ShouldBeEmpty();

        // Contracts solo contiene interfaces y DTOs, no implementaciones.
        ModuleTypes(null, ApplicationContracts)
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IApplicationService).IsAssignableFrom(t))
            .Select(t => t.FullName)
            .ShouldBeEmpty();
    }

    private static IEnumerable<Type> ModuleTypes(string? module, params Assembly[] assemblies) =>
        assemblies.SelectMany(a => a.GetTypes())
            .Where(t => t.Namespace is not null && !t.IsNested && !t.Name.StartsWith("<", StringComparison.Ordinal))
            .Where(t => module is null
                ? t.Namespace!.StartsWith(ModulesNamespace, StringComparison.Ordinal)
                : (t.Namespace + ".").StartsWith(ModulesNamespace + module + ".", StringComparison.Ordinal));

    private static List<string> DiscoverModules() =>
        Layers.SelectMany(a => a.GetTypes())
            .Select(t => t.Namespace)
            .Where(ns => ns is not null && ns.StartsWith(ModulesNamespace, StringComparison.Ordinal))
            .Select(ns => ns!.Substring(ModulesNamespace.Length).Split('.')[0])
            .Distinct()
            .OrderBy(x => x)
            .ToList();
}
