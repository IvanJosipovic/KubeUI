using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace KubeUI.App.Tests.Foundation.Localization;

/// <summary>
/// Contract tests for the JSON resource files that back the Lingua managers. The Lingua
/// generator flattens nested objects with underscores and ignores non-string leaves, so the
/// resource files must stay flat string maps with conventional keys.
/// </summary>
public sealed class JsonResourcesContractTests
{
    private static readonly string SourceRoot = GetSourceRoot();

    [Fact]
    public void json_resource_files_are_flat_string_maps_with_convention_keys()
    {
        var files = GetResourceFiles();

        files.ShouldNotBeEmpty("no Assets/Resources*.json files were found under src/");

        foreach (var file in files)
        {
            using var document = JsonDocument.Parse(File.ReadAllText(file));
            document.RootElement.ValueKind.ShouldBe(JsonValueKind.Object, file);

            var members = new List<KeyValuePair<string, JsonElement>>();
            foreach (var property in document.RootElement.EnumerateObject())
            {
                members.Add(new KeyValuePair<string, JsonElement>(property.Name, property.Value));
            }

            members.ShouldNotBeEmpty(file);

            foreach (var (key, value) in members)
            {
                value.ValueKind.ShouldBe(JsonValueKind.String, $"{file}: key '{key}'");
                value.GetString().ShouldNotBeNull($"{file}: key '{key}'");
                value.GetString()!.ShouldNotBeEmpty($"{file}: key '{key}'");
                key.ShouldMatch(@"^[A-Z][A-Za-z0-9]+(_[A-Za-z0-9]+)+$", $"{file}: key '{key}'");
            }
        }
    }

    [Fact]
    public void resource_key_owner_prefixes_match_types_in_the_owning_project()
    {
        var failures = new List<string>();

        foreach (var file in GetResourceFiles())
        {
            var projectDirectory = Directory.GetParent(Directory.GetParent(file)!.FullName)!.FullName;
            var declaredTypes = CollectDeclaredTypes(projectDirectory);
            using var document = JsonDocument.Parse(File.ReadAllText(file));

            foreach (var property in document.RootElement.EnumerateObject())
            {
                var owner = property.Name.Split('_', 2)[0];

                if (owner == "Shared")
                {
                    continue;
                }

                if (declaredTypes.Contains(owner))
                {
                    continue;
                }

                if (owner.EndsWith("PropertiesView", StringComparison.Ordinal) && owner.Length > "PropertiesView".Length)
                {
                    // Codified pattern: per-kind generated views named `<Kind>PropertiesView`
                    // whose class is always `PropertiesView` (see AGENTS.md).
                    continue;
                }

                failures.Add($"{file}: key '{property.Name}' owner prefix '{owner}' is not a type declared in {Path.GetFileName(projectDirectory)}, 'Shared', or a '<Kind>PropertiesView' pattern.");
            }
        }

        failures.ShouldBeEmpty(string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void resource_keys_are_referenced_from_source()
    {
        var sources = new List<string>();
        foreach (var root in new[] { SourceRoot, Path.Combine(Path.GetDirectoryName(SourceRoot)!, "tests") })
        {
            foreach (var path in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                if (path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                    || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                {
                    continue;
                }

                sources.Add(File.ReadAllText(path));
            }
        }

        var failures = new List<string>();

        foreach (var file in GetResourceFiles())
        {
            using var document = JsonDocument.Parse(File.ReadAllText(file));

            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (!sources.Any(source => source.Contains(property.Name, StringComparison.Ordinal)))
                {
                    failures.Add($"{file}: key '{property.Name}' is not referenced by any source file (dead key).");
                }
            }
        }

        failures.ShouldBeEmpty(string.Join(Environment.NewLine, failures));
    }

    private static string[] GetResourceFiles()
    {
        return Directory.GetFiles(SourceRoot, "Resources*.json", SearchOption.AllDirectories)
            .Where(static path => path.Contains($"{Path.DirectorySeparatorChar}Assets{Path.DirectorySeparatorChar}"))
            .Where(static path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();
    }

    private static HashSet<string> CollectDeclaredTypes(string projectDirectory)
    {
        var types = new HashSet<string>(StringComparer.Ordinal);
        var declarationStart = new Regex(@"\b(?:class|record|struct|enum|interface)\s+([A-Z][A-Za-z0-9]*)", RegexOptions.Compiled);

        foreach (var path in Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories))
        {
            if (path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            {
                continue;
            }

            foreach (Match match in declarationStart.Matches(File.ReadAllText(path)))
            {
                types.Add(match.Groups[1].Value);
            }
        }

        return types;
    }

    [Fact]
    public void source_tree_contains_no_resx_files()
    {
        var files = Directory.GetFiles(SourceRoot, "*.resx", SearchOption.AllDirectories)
            .Where(static path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .ToArray();

        files.ShouldBeEmpty("resx files must be replaced by Assets/Resources*.json");
    }

    private static string GetSourceRoot([CallerFilePath] string? callerFilePath = null)
    {
        callerFilePath.ShouldNotBeNull();

        var directory = Path.GetDirectoryName(Path.GetFullPath(callerFilePath!));
        while (directory is not null && !File.Exists(Path.Combine(directory, "KubeUI.slnx")))
        {
            directory = Path.GetDirectoryName(directory);
        }

        directory.ShouldNotBeNull("could not locate the repository root from the test file path");

        return Path.Combine(directory!, "src");
    }
}
