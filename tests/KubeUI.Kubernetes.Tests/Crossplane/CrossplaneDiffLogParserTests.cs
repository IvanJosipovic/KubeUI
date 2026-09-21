using KubeUI.Kubernetes;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Crossplane;

public sealed class CrossplaneDiffLogParserTests
{
    [Fact]
    public void Parses_managed_resource_identity_and_all_attribute_flags()
    {
        var line = "2026-09-21T17:56:46Z DEBUG provider-databricks Diff detected {\"uid\":\"uid-1\",\"name\":\"pipeline\",\"namespace\":\"data\",\"gvk\":\"compute.databricks.m.crossplane.io/v1beta1, Kind=Pipeline\",\"instanceDiff\":\"*terraform.InstanceDiff{Attributes:map[string]*terraform.ResourceAttrDiff{\\\"latest_updates.2.creation_time\\\":*terraform.ResourceAttrDiff{Old:\\\"2026-09-21T16:52:40.093Z\\\", New:\\\"\\\", NewComputed:false, NewRemoved:true, RequiresNew:false, Sensitive:false}}}\"}";

        var records = new CrossplaneDiffLogParser().Parse(line);

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
    public void Ignores_unrelated_and_malformed_lines()
    {
        var parser = new CrossplaneDiffLogParser();

        parser.Parse("normal provider log").ShouldBeEmpty();
        parser.Parse("DEBUG provider Diff detected {bad-json").ShouldBeEmpty();
    }

    [Fact]
    public void Aggregates_repeated_field_and_counts_distinct_instances()
    {
        var aggregator = new CrossplaneDiffAggregator();
        var record = new CrossplaneDiffRecord("uid-1", "one", "data", "example/v1", "Widget", "spec.value", "a", "b", false, false, false, false);

        aggregator.Add(record).ShouldBeTrue();
        aggregator.Add(record).ShouldBeFalse();
        aggregator.Add(record with { Uid = "uid-2", Name = "two" }).ShouldBeTrue();

        aggregator.Rows.Count.ShouldBe(2);
        aggregator.Rows.Single(row => row.Uid == "uid-1").Occurrences.ShouldBe(2);
        aggregator.Rows.Single(row => row.Uid == "uid-1").InstanceCount.ShouldBe(2);
        aggregator.GetInstanceCount("example/v1", "Widget", "spec.value").ShouldBe(2);
    }
}
