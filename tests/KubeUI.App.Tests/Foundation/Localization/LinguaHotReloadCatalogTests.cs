using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.RegularExpressions;
using Irihi.Lingua;
using KubeUI.Features.AI.Assets;
using KubeUI.Features.Crossplane.Assets;
using KubeUI.Features.GatewayApi.Assets;
using KubeUI.Features.Helm.Assets;
using KubeUI.Features.Resources.Assets;
using Shouldly;
using Xunit;
#if DEBUG
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using KubeUI.Foundation.Localization;
#endif

namespace KubeUI.App.Tests.Foundation.Localization;

public sealed class LinguaHotReloadCatalogTests
{
    [Fact]
    public void compiled_catalogs_include_hot_reload_only_in_debug()
    {
        var sourceRoot = JsonResourcesContractTests.GetSourceRoot();
        var framework = new DirectoryInfo(AppContext.BaseDirectory).Name;
#if DEBUG
        const string configuration = "Debug";
#else
        const string configuration = "Release";
#endif
        foreach (var (project, managerName) in ProductionManagers())
        {
            var path = Path.Combine(sourceRoot, project, "bin", configuration, framework, project + ".dll");
            using var stream = File.OpenRead(path);
            using var pe = new PEReader(stream);
            var reader = pe.GetMetadataReader();
            var types = reader.TypeDefinitions.Select(handle => reader.GetString(reader.GetTypeDefinition(handle).Name)).ToArray();
            var managerType = reader.TypeDefinitions.Select(reader.GetTypeDefinition)
                .Single(type => reader.GetString(type.Name) == managerName);
            var managerTypeName = reader.GetString(managerType.Namespace) + "." + managerName;
            var methods = managerType.GetMethods().Select(handle => reader.GetString(reader.GetMethodDefinition(handle).Name)).ToArray();
            var registrations = new List<string>();
            foreach (var handle in reader.GetAssemblyDefinition().GetCustomAttributes())
            {
                var attribute = reader.GetCustomAttribute(handle);
                if (attribute.Constructor.Kind != HandleKind.MemberReference)
                {
                    continue;
                }

                var parent = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor).Parent;
                if (parent.Kind != HandleKind.TypeReference)
                {
                    continue;
                }

                var type = reader.GetTypeReference((TypeReferenceHandle)parent);
                if (reader.GetString(type.Name) != "MetadataUpdateHandlerAttribute")
                {
                    continue;
                }

                var blob = reader.GetBlobReader(attribute.Value);
                blob.ReadUInt16().ShouldBe((ushort)1);
                var handler = blob.ReadSerializedString();
                if (handler is not null
                    && Regex.IsMatch(handler, @"(?:^|\[)" + Regex.Escape(managerTypeName) + @"(?:,|\]|$)"))
                {
                    registrations.Add(handler);
                }
            }

#if DEBUG
            methods.ShouldContain("KubeUI.Foundation.Localization.ILinguaHotReload.Reload", project);
            registrations.Count.ShouldBe(1, project);
            registrations[0].ShouldStartWith("KubeUI.Foundation.Localization.LinguaHotReload`1[");
            if (project == "KubeUI.Foundation")
            {
                types.ShouldContain("LinguaHotReload");
                types.ShouldContain("LinguaHotReload`1");
                types.ShouldContain("ILinguaHotReload");
            }
#else
            types.ShouldNotContain("HotReloadHandler", project);
            types.ShouldNotContain("LinguaHotReload", project);
            types.ShouldNotContain("LinguaHotReload`1", project);
            types.ShouldNotContain("ILinguaHotReload", project);
            methods.ShouldNotContain("UpdateApplication", project);
            methods.ShouldNotContain("KubeUI.Foundation.Localization.ILinguaHotReload.Reload", project);
            var references = reader.TypeReferences.Select(handle => reader.GetString(reader.GetTypeReference(handle).Name)).ToArray();
            references.ShouldNotContain("LinguaHotReload", project);
            references.ShouldNotContain("LinguaHotReload`1", project);
            references.ShouldNotContain("ILinguaHotReload", project);
            registrations.ShouldBeEmpty(project);
#endif
        }
    }

#if DEBUG
    [AvaloniaFact]
    public void every_production_manager_has_a_working_statically_registered_refresh()
    {
        var catalogs = Catalogs();
        var declarations = ProductionManagers().Select(item => item.Project + ":" + item.Name).Order().ToArray();
        catalogs.Select(item => item.Project + ":" + item.Name).Order().ShouldBe(declarations);

        foreach (var (project, _, resolve, refresh) in catalogs)
        {
            var manager = resolve();
            var culture = manager.CurrentCulture;
            using var resources = JsonDocument.Parse(File.ReadAllText(
                Path.Combine(JsonResourcesContractTests.GetSourceRoot(), project, "Assets", "Resources.json")));
            var entry = resources.RootElement.EnumerateObject().First();
            var observable = (LinguaObservableString)manager.GetObservable(entry.Name)!;
            observable.ShouldNotBeNull(project);
            try
            {
                manager.UpdateCulture(CultureInfo.InvariantCulture);
                observable.OnNext("stale value before metadata refresh");

                refresh();
                Dispatcher.UIThread.RunJobs();

                manager.GetObservable(entry.Name).ShouldBeSameAs(observable, project);
                observable.CurrentValue.ShouldBe(entry.Value.GetString(), project);
                manager.CurrentCulture.ShouldBe(CultureInfo.InvariantCulture, project);
            }
            finally
            {
                manager.UpdateCulture(culture);
            }
        }
    }

    private static (string Project, string Name, Func<ILinguaManager> Resolve, Action Refresh)[] Catalogs() =>
    [
        ("KubeUI.Foundation", nameof(FoundationLanguageManager), static () => FoundationLanguageManager.Instance,
            static () => LinguaHotReload<FoundationLanguageManager>.UpdateApplication([typeof(FoundationLanguageManager)])),
        ("KubeUI.App", nameof(AppLanguageManager), static () => AppLanguageManager.Instance,
            static () => LinguaHotReload<AppLanguageManager>.UpdateApplication([typeof(AppLanguageManager)])),
        ("KubeUI.Features.AI", nameof(AiLanguageManager), static () => AiLanguageManager.Instance,
            static () => LinguaHotReload<AiLanguageManager>.UpdateApplication([typeof(AiLanguageManager)])),
        ("KubeUI.Features.Clusters", nameof(ClustersLanguageManager), static () => ClustersLanguageManager.Instance,
            static () => LinguaHotReload<ClustersLanguageManager>.UpdateApplication([typeof(ClustersLanguageManager)])),
        ("KubeUI.Features.Resources", nameof(ResourcesLanguageManager), static () => ResourcesLanguageManager.Instance,
            static () => LinguaHotReload<ResourcesLanguageManager>.UpdateApplication([typeof(ResourcesLanguageManager)])),
        ("KubeUI.Features.Cilium", nameof(CiliumLanguageManager), static () => CiliumLanguageManager.Instance,
            static () => LinguaHotReload<CiliumLanguageManager>.UpdateApplication([typeof(CiliumLanguageManager)])),
        ("KubeUI.Features.Crossplane", nameof(CrossplaneLanguageManager), static () => CrossplaneLanguageManager.Instance,
            static () => LinguaHotReload<CrossplaneLanguageManager>.UpdateApplication([typeof(CrossplaneLanguageManager)])),
        ("KubeUI.Features.GatewayApi", nameof(GatewayApiLanguageManager), static () => GatewayApiLanguageManager.Instance,
            static () => LinguaHotReload<GatewayApiLanguageManager>.UpdateApplication([typeof(GatewayApiLanguageManager)])),
        ("KubeUI.Features.Helm", nameof(HelmLanguageManager), static () => HelmLanguageManager.Instance,
            static () => LinguaHotReload<HelmLanguageManager>.UpdateApplication([typeof(HelmLanguageManager)]))
    ];
#endif

    private static (string Project, string Name)[] ProductionManagers()
    {
        var sourceRoot = JsonResourcesContractTests.GetSourceRoot();
        var managers = new List<(string, string)>();
        foreach (var path in Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceRoot, path);
            var segments = relative.Split(Path.DirectorySeparatorChar);
            if (segments.Contains("obj") || segments.Contains("bin"))
            {
                continue;
            }

            var source = File.ReadAllText(path);
            if (!source.Contains("[LinguaManager(", StringComparison.Ordinal))
            {
                continue;
            }

            var declaration = Regex.Match(source, @"partial class (\w+)");
            declaration.Success.ShouldBeTrue(path);
            managers.Add((segments[0], declaration.Groups[1].Value));
        }

        managers.ShouldNotBeEmpty();
        return managers.ToArray();
    }
}
