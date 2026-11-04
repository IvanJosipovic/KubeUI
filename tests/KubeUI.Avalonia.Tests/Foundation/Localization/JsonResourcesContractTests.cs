using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Shouldly;
using Xunit;

namespace KubeUI.Avalonia.Tests.Foundation.Localization;

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
        var files = Directory.GetFiles(SourceRoot, "Resources*.json", SearchOption.AllDirectories)
            .Where(static path => path.Contains($"{Path.DirectorySeparatorChar}Assets{Path.DirectorySeparatorChar}"))
            .Where(static path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

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
