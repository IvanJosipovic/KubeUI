using System.Text;
using KubeUI.Features.Resources.Workloads.v1.Pod.Services;
using Shouldly;

namespace KubeUI.Features.Resources.Tests.Workloads.v1.Pod;

public sealed class ContainerFileListingParserTests
{
    [Fact]
    public void parses_entries_with_type_path_and_size()
    {
        var output = Bytes("d\0/etc\0" + "4096\0" + "f\0/etc/hosts\0" + "220\0");

        var listing = ContainerFileListingParser.Parse(output, 100);

        listing.Truncated.ShouldBeFalse();
        listing.Entries.Count.ShouldBe(2);
        listing.Entries[0].Name.ShouldBe("etc");
        listing.Entries[0].IsDirectory.ShouldBeTrue();
        listing.Entries[0].Size.ShouldBe(4096);
        listing.Entries[1].Name.ShouldBe("hosts");
        listing.Entries[1].IsDirectory.ShouldBeFalse();
        listing.Entries[1].Size.ShouldBe(220);
    }

    [Fact]
    public void orders_directories_before_files()
    {
        var output = Bytes("f\0/a.txt\0" + "1\0" + "d\0/zdir\0" + "2\0");

        var listing = ContainerFileListingParser.Parse(output, 100);

        listing.Entries[0].Name.ShouldBe("zdir");
        listing.Entries[1].Name.ShouldBe("a.txt");
    }

    [Fact]
    public void keeps_names_with_spaces_and_newlines()
    {
        var output = Bytes("f\0/a b\0" + "1\0" + "f\0/a\nb\0" + "2\0");

        var listing = ContainerFileListingParser.Parse(output, 100);

        listing.Entries.Select(static e => e.Name).ShouldBe(["a\nb", "a b"]);
    }

    [Fact]
    public void marks_symbolic_links()
    {
        var output = Bytes("l\0/link\0" + "0\0");

        var listing = ContainerFileListingParser.Parse(output, 100);

        listing.Entries[0].IsSymbolicLink.ShouldBeTrue();
        listing.Entries[0].IsDirectory.ShouldBeFalse();
    }

    [Fact]
    public void truncates_at_the_entry_limit()
    {
        var builder = new StringBuilder();
        for (var i = 0; i < 10; i++)
        {
            builder.Append($"f\0/file{i}\0").Append("1\0");
        }

        var listing = ContainerFileListingParser.Parse(Bytes(builder.ToString()), 3);

        listing.Truncated.ShouldBeTrue();
        listing.Entries.Count.ShouldBe(3);
    }

    [Fact]
    public void ignores_incomplete_trailing_fields()
    {
        var output = Bytes("f\0/a\0" + "1\0" + "f\0/b");

        var listing = ContainerFileListingParser.Parse(output, 100);

        listing.Entries.Count.ShouldBe(1);
        listing.Entries[0].Name.ShouldBe("a");
    }

    [Fact]
    public void drops_a_field_cut_off_inside_an_entry()
    {
        // The byte limit cut the size field of the second entry after "12".
        var output = Bytes("f\0/a\0" + "1\0" + "f\0/b\0" + "12");

        var listing = ContainerFileListingParser.Parse(output, 100);

        listing.Entries.Count.ShouldBe(1);
        listing.Entries[0].Name.ShouldBe("a");
        listing.Entries[0].Size.ShouldBe(1);
    }

    [Fact]
    public void reports_empty_output_as_no_entries()
    {
        var listing = ContainerFileListingParser.Parse([], 100);

        listing.Entries.ShouldBeEmpty();
        listing.Truncated.ShouldBeFalse();
    }

    [Fact]
    public void detects_binary_content()
    {
        ContainerFileListingParser.LooksLikeText(Bytes("hello")).ShouldBeTrue();
        ContainerFileListingParser.LooksLikeText([0x00, 0x01, 0x02]).ShouldBeFalse();
        ContainerFileListingParser.LooksLikeText([]).ShouldBeTrue();
    }

    private static byte[] Bytes(string text) => Encoding.UTF8.GetBytes(text);
}
