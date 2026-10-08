using System.Text.Json;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Crossplane;

public sealed class CrossplaneDiffLogParserTests
{
    [Fact]
    public void Parses_managed_resource_identity_and_all_attribute_flags()
    {
        var line = "2026-09-21T17:56:46Z DEBUG provider-databricks Diff detected {\"uid\":\"uid-1\",\"name\":\"pipeline\",\"namespace\":\"data\",\"gvk\":\"compute.databricks.m.crossplane.io/v1beta1, Kind=Pipeline\",\"instanceDiff\":\"*terraform.InstanceDiff{Attributes:map[string]*terraform.ResourceAttrDiff{\\\"latest_updates.2.creation_time\\\":*terraform.ResourceAttrDiff{Old:\\\"2026-09-21T16:52:40.093Z\\\", New:\\\"\\\", NewComputed:false, NewRemoved:true, RequiresNew:false, Sensitive:false}}}\"}";

        var records = new CrossplaneDiffLogParser().Parse(line, out var wasTruncated);

        wasTruncated.ShouldBeFalse();
        records.Count.ShouldBe(1);
        var record = records[0];
        record.Uid.ShouldBe("uid-1");
        record.Name.ShouldBe("pipeline");
        record.Namespace.ShouldBe("data");
        record.ApiVersion.ShouldBe("compute.databricks.m.crossplane.io/v1beta1");
        record.Kind.ShouldBe("Pipeline");
        record.DiffField.ShouldBe("latest_updates.2.creation_time");
        record.OldValue.ShouldBe("2026-09-21T16:52:40.093Z");
        record.NewValue.ShouldBe(string.Empty);
        record.NewRemoved.ShouldBeTrue();
        record.NewComputed.ShouldBeFalse();
    }

    [Fact]
    public void Parses_multiple_attributes_with_observed_value_and_flag_permutations()
    {
        var instanceDiff = """*terraform.InstanceDiff{Attributes:map[string]*terraform.ResourceAttrDiff{"empty":*terraform.ResourceAttrDiff{Old:"", New:"", NewComputed:false, NewRemoved:false, RequiresNew:false, Sensitive:false},"newOnly":*terraform.ResourceAttrDiff{Old:"", New:"new"},"replacement":*terraform.ResourceAttrDiff{Old:"old", New:"new"},"requiresNew":*terraform.ResourceAttrDiff{Old:"", New:"new", RequiresNew:true},"requiresNewWithOld":*terraform.ResourceAttrDiff{Old:"old", New:"new", RequiresNew:true},"removed":*terraform.ResourceAttrDiff{Old:"", New:"", NewRemoved:true},"removedWithOld":*terraform.ResourceAttrDiff{Old:"old", New:"", NewRemoved:true},"removedWithValues":*terraform.ResourceAttrDiff{Old:"old", New:"new", NewRemoved:true},"computed":*terraform.ResourceAttrDiff{Old:"", New:"", NewComputed:true},"computedRequiresNew":*terraform.ResourceAttrDiff{Old:"", New:"", NewComputed:true, RequiresNew:true}}}""";

        var records = new CrossplaneDiffLogParser().Parse(CreateLine(instanceDiff)).ToDictionary(record => record.DiffField);

        records.Count.ShouldBe(10);
        records["empty"].OldValue.ShouldBeEmpty();
        records["empty"].NewValue.ShouldBeEmpty();
        records["newOnly"].OldValue.ShouldBeEmpty();
        records["newOnly"].NewValue.ShouldBe("new");
        records["replacement"].OldValue.ShouldBe("old");
        records["replacement"].NewValue.ShouldBe("new");
        records["requiresNew"].RequiresNew.ShouldBeTrue();
        records["requiresNewWithOld"].RequiresNew.ShouldBeTrue();
        records["removed"].NewRemoved.ShouldBeTrue();
        records["removedWithOld"].NewRemoved.ShouldBeTrue();
        records["removedWithValues"].NewRemoved.ShouldBeTrue();
        records["computed"].NewComputed.ShouldBeTrue();
        records["computedRequiresNew"].NewComputed.ShouldBeTrue();
        records["computedRequiresNew"].RequiresNew.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("*")]
    [InlineData("terraform.")]
    [InlineData("*terraform.")]
    public void Parses_supported_resource_attribute_type_prefixes(string prefix)
    {
        var attributeType = $"{prefix}ResourceAttrDiff";
        var instanceDiff = "*terraform.InstanceDiff{Attributes:map[string]*terraform.ResourceAttrDiff{\"field\":"
            + attributeType
            + "{Old:\"old\", New:\"new\"}}}";

        var record = new CrossplaneDiffLogParser().Parse(CreateLine(instanceDiff)).ShouldHaveSingleItem();

        record.OldValue.ShouldBe("old");
        record.NewValue.ShouldBe("new");
    }

    [Fact]
    public void Ignores_unsupported_resource_attribute_type_prefixes()
    {
        var instanceDiff = """*terraform.InstanceDiff{Attributes:map[string]*terraform.ResourceAttrDiff{"field":*other.ResourceAttrDiff{Old:"old", New:"new"}}}""";

        new CrossplaneDiffLogParser().Parse(CreateLine(instanceDiff)).ShouldBeEmpty();
    }

    [Fact]
    public void Nil_attribute_does_not_take_the_next_attribute_diff()
    {
        var instanceDiff = """*terraform.InstanceDiff{Attributes:map[string]*terraform.ResourceAttrDiff{"unchanged":nil,"changed":*terraform.ResourceAttrDiff{Old:"before", New:"after"}}}""";

        var record = new CrossplaneDiffLogParser().Parse(CreateLine(instanceDiff)).ShouldHaveSingleItem();

        record.DiffField.ShouldBe("changed");
        record.OldValue.ShouldBe("before");
        record.NewValue.ShouldBe("after");
    }

    [Fact]
    public void Recovers_complete_attributes_from_a_truncated_provider_log_line()
    {
        var longValue = new string('x', 20_000);
        var instanceDiff = "*terraform.InstanceDiff{Attributes:map[string]*terraform.ResourceAttrDiff{\"first\":*terraform.ResourceAttrDiff{Old:\"old-1\", New:\"new-1\"},\"second\":*terraform.ResourceAttrDiff{Old:\"old-2\", New:\"new-2\"},\"truncated\":*terraform.ResourceAttrDiff{Old:\""
            + longValue
            + "\", New:\"new-3\"}}}";
        var line = CreateLine(instanceDiff)[..16_384];

        var records = new CrossplaneDiffLogParser().Parse(line, out var wasTruncated);

        wasTruncated.ShouldBeTrue();
        records.Select(record => record.DiffField).ShouldBe(["first", "second"]);
        records.Select(record => record.NewValue).ShouldBe(["new-1", "new-2"]);
    }

    [Fact]
    public void Recovers_complete_attributes_when_the_diff_string_is_truncated_inside_valid_json()
    {
        var instanceDiff = """*terraform.InstanceDiff{Attributes:map[string]*terraform.ResourceAttrDiff{"complete":*terraform.ResourceAttrDiff{Old:"before", New:"after"}""";

        var records = new CrossplaneDiffLogParser().Parse(CreateLine(instanceDiff), out var wasTruncated);

        wasTruncated.ShouldBeTrue();
        records.ShouldHaveSingleItem().DiffField.ShouldBe("complete");
    }

    [Fact]
    public void Ignores_unrelated_and_malformed_lines()
    {
        var parser = new CrossplaneDiffLogParser();

        parser.Parse("normal provider log").ShouldBeEmpty();
        parser.Parse("DEBUG provider Diff detected {bad-json").ShouldBeEmpty();
    }

    [Fact]
    public void Ignores_valid_json_values_that_are_not_diff_objects()
    {
        new CrossplaneDiffLogParser().Parse("DEBUG provider Diff detected []").ShouldBeEmpty();
    }

    [Fact]
    public void Ignores_empty_lines_and_incomplete_diff_payloads()
    {
        var parser = new CrossplaneDiffLogParser();

        parser.Parse(string.Empty).ShouldBeEmpty();
        parser.Parse("DEBUG provider Diff detected").ShouldBeEmpty();
        parser.Parse(CreateLine("no attributes")).ShouldBeEmpty();
        parser.Parse(CreateLine("Attributes:map[string]*terraform.ResourceAttrDiff")).ShouldBeEmpty();
        parser.Parse(CreateLine("""*terraform.InstanceDiff{Attributes:map[string]*terraform.ResourceAttrDiff{"field":nil}}""")).ShouldBeEmpty();
    }

    [Fact]
    public void Ignores_missing_required_string_properties_and_invalid_gvk()
    {
        var parser = new CrossplaneDiffLogParser();
        var incompletePayload = JsonSerializer.Serialize(new
        {
            name = "widget",
            @namespace = "default",
            gvk = "example.com/v1, Kind=Widget",
            instanceDiff = """*terraform.InstanceDiff{Attributes:map[string]*terraform.ResourceAttrDiff{"field":*terraform.ResourceAttrDiff{Old:"old"}}}"""
        });

        parser.Parse($"DEBUG provider Diff detected {incompletePayload}").ShouldBeEmpty();
        parser.Parse(CreateLine("""*terraform.InstanceDiff{Attributes:map[string]*terraform.ResourceAttrDiff{"field":*terraform.ResourceAttrDiff{Old:"old"}}}""", gvk: "example.com/v1")).ShouldBeEmpty();

        foreach (var gvk in new[] { ", Kind=Widget", " , Kind=Widget", "example.com/v1, Kind=", "example.com/v1, Kind= " })
        {
            parser.Parse(CreateLine("""*terraform.InstanceDiff{Attributes:map[string]*terraform.ResourceAttrDiff{"field":*terraform.ResourceAttrDiff{Old:"old"}}}""", gvk)).ShouldBeEmpty();
        }

        var requiredProperties = new Dictionary<string, object>
        {
            ["uid"] = "uid-1",
            ["name"] = "widget",
            ["namespace"] = "default",
            ["gvk"] = "example.com/v1, Kind=Widget",
            ["instanceDiff"] = """*terraform.InstanceDiff{Attributes:map[string]*terraform.ResourceAttrDiff{"field":*terraform.ResourceAttrDiff{Old:"old"}}}"""
        };
        foreach (var property in requiredProperties.Keys.ToArray())
        {
            var payload = new Dictionary<string, object>(requiredProperties)
            {
                [property] = 1
            };
            parser.Parse($"DEBUG provider Diff detected {JsonSerializer.Serialize(payload)}").ShouldBeEmpty();
        }
    }

    [Fact]
    public void Redacts_sensitive_attribute_values_during_parsing()
    {
        var line = "DEBUG provider Diff detected {\"uid\":\"uid-1\",\"name\":\"db\",\"namespace\":\"default\",\"gvk\":\"example.com/v1, Kind=Database\",\"instanceDiff\":\"*terraform.InstanceDiff{Attributes:map[string]*terraform.ResourceAttrDiff{\\\"spec.password\\\":*terraform.ResourceAttrDiff{Old:\\\"old-secret\\\", New:\\\"new-secret\\\", Sensitive:true}}}\"}";

        var record = new CrossplaneDiffLogParser().Parse(line).ShouldHaveSingleItem();

        record.Sensitive.ShouldBeTrue();
        record.OldValue.ShouldBeEmpty();
        record.NewValue.ShouldBeEmpty();
    }

    [Fact]
    public void Decodes_go_string_escapes_in_diff_values()
    {
        var instanceDiff = """*terraform.InstanceDiff{Attributes:map[string]*terraform.ResourceAttrDiff{"spec.value":*terraform.ResourceAttrDiff{Old:"before\x3c\u003c\141\a\b\f\r\t\v\\\"", New:"line\nnext\U0001F642", NewComputed:true, NewRemoved:true, RequiresNew:true}}}""";
        var payload = JsonSerializer.Serialize(new
        {
            uid = "uid-1",
            name = "widget",
            @namespace = "default",
            gvk = "example.com/v1, Kind=Widget",
            instanceDiff
        });

        var record = new CrossplaneDiffLogParser().Parse($"DEBUG provider Diff detected {payload}").ShouldHaveSingleItem();

        record.OldValue.ShouldBe("before<<a\a\b\f\r\t\v\\\"");
        record.NewValue.ShouldBe(string.Concat("line\nnext", char.ConvertFromUtf32(0x1F642)));
        record.NewComputed.ShouldBeTrue();
        record.NewRemoved.ShouldBeTrue();
        record.RequiresNew.ShouldBeTrue();
    }

    [Theory]
    [InlineData("\\xG0")]
    [InlineData("\\uD800")]
    [InlineData("\\U00110000")]
    [InlineData("\\400")]
    [InlineData("\\08")]
    [InlineData("\\q")]
    public void Invalid_go_escapes_do_not_return_partial_attribute_values(string invalidEscape)
    {
        var instanceDiff = "*terraform.InstanceDiff{Attributes:map[string]*terraform.ResourceAttrDiff{\"spec.value\":*terraform.ResourceAttrDiff{Old:\""
            + invalidEscape
            + "\", New:\"new\"}}}";

        var record = new CrossplaneDiffLogParser().Parse(CreateLine(instanceDiff)).ShouldHaveSingleItem();

        record.OldValue.ShouldBeEmpty();
        record.NewValue.ShouldBe("new");
    }

    [Fact]
    public void Aggregates_repeated_field_and_counts_distinct_instances()
    {
        var aggregator = new CrossplaneDiffAggregator();
        var record = new CrossplaneDiffRecord("uid-1", "one", "data", "example/v1", "Widget", "spec.value", "a", "b", false, false, false);

        aggregator.Add(record).ShouldBeTrue();
        aggregator.AddAndGetAffectedRows(record).Count.ShouldBe(1);
        aggregator.AddAndGetAffectedRows(record with { Uid = "uid-2", Name = "two" }).Count.ShouldBe(2);
        aggregator.AddAndGetAffectedRows(record).Count.ShouldBe(1);

        aggregator.Rows.Count.ShouldBe(2);
        aggregator.Rows.Single(row => row.Uid == "uid-1").Occurrences.ShouldBe(3);
        aggregator.Rows.Single(row => row.Uid == "uid-1").InstanceCount.ShouldBe(2);
        aggregator.GetInstanceCount("example/v1", "Widget", "spec.value").ShouldBe(2);
    }

    [Fact]
    public void Row_snapshots_observe_group_counts_without_copying_every_existing_row()
    {
        var aggregator = new CrossplaneDiffAggregator();
        var first = new CrossplaneDiffRecord("uid-1", "one", "data", "example/v1", "Widget", "spec.value", "a", "b", false, false, false);
        var firstSnapshot = aggregator.AddAndGetRowSnapshot(first, out var firstAdded).ShouldNotBeNull();

        var secondSnapshot = aggregator.AddAndGetRowSnapshot(first with { Uid = "uid-2", Name = "two" }, out var secondAdded).ShouldNotBeNull();

        firstAdded.ShouldBeTrue();
        secondAdded.ShouldBeTrue();
        firstSnapshot.InstanceCount.ShouldBe(2);
        secondSnapshot.InstanceCount.ShouldBe(2);
        aggregator.GetGroupSnapshot("example/v1", "Widget", "spec.value").Count.ShouldBe(2);
    }

    [Fact]
    public void Aggregator_bounds_new_rows_but_continues_updating_retained_rows()
    {
        var aggregator = new CrossplaneDiffAggregator(maximumRowCount: 1);
        var retained = new CrossplaneDiffRecord("uid-1", "one", "data", "example/v1", "Widget", "spec.value", "a", "b", false, false, false);
        var omitted = retained with { Uid = "uid-2", Name = "two" };

        var firstSnapshot = aggregator.AddAndGetRowSnapshot(retained, out var firstAdded).ShouldNotBeNull();
        var updatedSnapshot = aggregator.AddAndGetRowSnapshot(retained with { NewValue = "updated" }, out var updateAdded).ShouldNotBeNull();
        var omittedSnapshot = aggregator.AddAndGetRowSnapshot(omitted, out var omittedAdded);

        firstAdded.ShouldBeTrue();
        updateAdded.ShouldBeFalse();
        omittedAdded.ShouldBeFalse();
        firstSnapshot.InstanceCount.ShouldBe(1);
        updatedSnapshot.NewValue.ShouldBe("updated");
        updatedSnapshot.Occurrences.ShouldBe(2);
        omittedSnapshot.ShouldBeNull();
        aggregator.AddAndGetAffectedRows(omitted).ShouldBeEmpty();
        aggregator.Add(omitted).ShouldBeFalse();
        aggregator.Rows.Count.ShouldBe(1);

        aggregator.Clear();
        aggregator.AddAndGetRowSnapshot(omitted, out var addedAfterClear).ShouldNotBeNull();
        addedAfterClear.ShouldBeTrue();
        aggregator.Rows.ShouldHaveSingleItem().Uid.ShouldBe("uid-2");
    }

    [Fact]
    public void Snapshots_are_detached_and_clear_releases_aggregated_rows()
    {
        var aggregator = new CrossplaneDiffAggregator();
        var record = new CrossplaneDiffRecord("uid-1", "one", "data", "example/v1", "Widget", "spec.value", "a", "b", false, false, false);
        aggregator.Add(record);

        var snapshot = aggregator.GetSnapshot().ShouldHaveSingleItem();
        aggregator.Add(record with { NewValue = "updated" });

        snapshot.NewValue.ShouldBe("b");
        aggregator.Rows.Single().NewValue.ShouldBe("updated");
        aggregator.Rows.Single().Occurrences.ShouldBe(2);

        aggregator.Clear();

        aggregator.Rows.ShouldBeEmpty();
        aggregator.GetSnapshot().ShouldBeEmpty();
        aggregator.GetInstanceCount("example/v1", "Widget", "spec.value").ShouldBe(0);
        aggregator.GetGroupSnapshot("example/v1", "Widget", "spec.value").ShouldBeEmpty();
    }

    [Fact]
    public void Sensitive_rows_never_retain_attribute_values()
    {
        var record = new CrossplaneDiffRecord(
            "uid-1",
            "one",
            "data",
            "example/v1",
            "Widget",
            "spec.password",
            "old-secret",
            "new-secret",
            false,
            false,
            false,
            Sensitive: true);
        var aggregator = new CrossplaneDiffAggregator();

        var row = aggregator.AddAndGetRowSnapshot(record, out _).ShouldNotBeNull();

        row.Sensitive.ShouldBeTrue();
        row.OldValue.ShouldBeEmpty();
        row.NewValue.ShouldBeEmpty();
        aggregator.GetSnapshot().ShouldHaveSingleItem().OldValue.ShouldBeEmpty();

        aggregator.Add(record with
        {
            OldValue = "old-secret-after-update",
            NewValue = "new-secret-after-update",
            Sensitive = false
        });

        var updatedRow = aggregator.GetSnapshot().ShouldHaveSingleItem();
        updatedRow.Sensitive.ShouldBeTrue();
        updatedRow.OldValue.ShouldBeEmpty();
        updatedRow.NewValue.ShouldBeEmpty();
    }

    private static string CreateLine(string instanceDiff, string gvk = "example.com/v1, Kind=Widget")
    {
        var payload = JsonSerializer.Serialize(new
        {
            uid = "uid-1",
            name = "widget",
            @namespace = "default",
            gvk,
            instanceDiff
        });
        return $"DEBUG provider Diff detected {payload}";
    }
}
