using System.Text.Json.Nodes;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using FluentAvalonia.UI.Controls;
using KubeUI.Avalonia.Features.Resources.Editor;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Features.Resources.Yaml;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Editor;

public sealed class ResourceEditorNodeControlTests
{
    [AvaloniaFact]
    public void Recursive_editor_renders_collapsible_sections_and_typed_editors()
    {
        var root = ResourceEditorNodeViewModel.Create(CreateSchema(), JsonNode.Parse("""
            {
              "spec": {
                "replicas": 2,
                "enabled": true,
                "mode": "Active",
                "note": "ready",
                "containers": [{ "name": "api" }],
                "labels": { "team": "platform" }
              }
            }
            """)!);
        var control = new ResourceEditorNodeControl(root);
        using var window = Application.Current.CreateTestWindow(900, 700, control);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var descendants = control.GetVisualDescendants().ToArray();
        descendants.OfType<ExpandableSection>().Count().ShouldBe(4);
        descendants.OfType<ExpandableSection>().Count(section => section.IsExpanded).ShouldBe(2);
        descendants.OfType<TextBox>().ShouldContain(editor => editor.Name == "NumberEditor");
        descendants.OfType<TextBox>().ShouldContain(editor => editor.Name == "StringEditor");
        descendants.OfType<CheckBox>().ShouldContain(editor => editor.Name == "BooleanEditor");
        descendants.OfType<ComboBox>().ShouldContain(editor => editor.Name == "EnumEditor");
        descendants.OfType<TextBlock>().ShouldNotContain(text => text.Text == "Container list");
        descendants.OfType<TextBlock>().ShouldNotContain(text => text.Text == "Labels");
        descendants.OfType<ExpandableSection>().Any(HasTooltip("Container list")).ShouldBeTrue();
        descendants.OfType<ExpandableSection>().Any(HasTooltip("Labels")).ShouldBeTrue();
        descendants.OfType<TextBox>().ShouldContain(editor => Equals(ToolTip.GetTip(editor), "Replica count"));
        descendants.OfType<ExpandableSection>().All(HasCenteredHeader).ShouldBeTrue();
        foreach (var grid in descendants.OfType<Grid>().Where(grid => grid.Name == "ScalarEditor"))
            grid.Margin.Bottom.ShouldBe(0);
    }

    [AvaloniaFact]
    public void Collection_commands_refresh_the_rendered_recursive_tree()
    {
        var root = ResourceEditorNodeViewModel.Create(CreateSchema(), JsonNode.Parse("""
            { "spec": { "replicas": 1, "enabled": true, "mode": "Active", "containers": [], "labels": {} } }
            """)!);
        var control = new ResourceEditorNodeControl(root);
        using var window = Application.Current.CreateTestWindow(900, 700, control);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        var spec = root.Children.Single(child => child.Name == "spec");
        var containers = spec.Children.Single(child => child.Name == "containers");
        var labels = spec.Children.Single(child => child.Name == "labels");

        containers.AddItemCommand.Execute(null);
        labels.NewMapKey = "owner";
        labels.AddMapEntryCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        containers.Children.ShouldHaveSingleItem();
        labels.Children.ShouldHaveSingleItem();
        containers.Children.Single().CanRemove.ShouldBeTrue();
        labels.Children.Single().CanRemove.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Read_only_fields_show_an_indicator_while_remaining_selectable()
    {
        var schema = ObjectSchema("root", new Dictionary<string, ResourceEditorSchemaNode>
        {
            ["apiVersion"] = ScalarSchema("apiVersion", ResourceEditorValueKind.String),
        });
        var root = ResourceEditorNodeViewModel.Create(schema, JsonNode.Parse("{\"apiVersion\":\"v1\"}")!);
        var control = new ResourceEditorNodeControl(root);
        using var window = Application.Current.CreateTestWindow(600, 300, control);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        control.GetVisualDescendants().OfType<TextBox>().ShouldBeEmpty();
        var editor = control.GetVisualDescendants().OfType<SelectableTextBlock>().Single();
        editor.Name.ShouldBe("ReadOnlyEditor");
        editor.Text.ShouldBe("v1");
    }

    [AvaloniaFact]
    public void Yaml_fields_initialize_the_editor_with_the_current_value()
    {
        var schema = new ResourceEditorSchemaNode(
            "values",
            ResourceEditorValueKind.String,
            null,
            new Dictionary<string, ResourceEditorSchemaNode>(StringComparer.Ordinal),
            null,
            [],
            new HashSet<string>(StringComparer.Ordinal),
            null,
            true);
        var node = ResourceEditorNodeViewModel.Create(
            schema,
            JsonValue.Create("allow:\n  enabled: false")!);

        var control = new ResourceEditorNodeControl(node);
        using var window = Application.Current.CreateTestWindow(900, 500, control);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        var editor = control.GetVisualDescendants().OfType<TextEditor>().Single();

        editor.Text.ShouldContain("allow:");
        editor.Text.ShouldContain("enabled: false");
        editor.TextArea.IndentationStrategy.ShouldBeOfType<YamlIndentationStrategy>();
        editor.Options.ConvertTabsToSpaces.ShouldBeTrue();
        editor.Options.IndentationSize.ShouldBe(2);
        editor.Options.EnableHyperlinks.ShouldBeFalse();
        editor.Options.HighlightCurrentLine.ShouldBeTrue();

        var bringIntoView = new RequestBringIntoViewEventArgs
        {
            RoutedEvent = Control.RequestBringIntoViewEvent,
            Source = editor,
        };
        editor.RaiseEvent(bringIntoView);
        bringIntoView.Handled.ShouldBeTrue();
    }

    private static ResourceEditorSchemaNode CreateSchema()
    {
        var container = ObjectSchema("container", new Dictionary<string, ResourceEditorSchemaNode>
        {
            ["name"] = ScalarSchema("name", ResourceEditorValueKind.String),
        });
        var spec = ObjectSchema("spec", new Dictionary<string, ResourceEditorSchemaNode>
        {
            ["replicas"] = ScalarSchema("replicas", ResourceEditorValueKind.Number, description: "Replica count"),
            ["enabled"] = ScalarSchema("enabled", ResourceEditorValueKind.Boolean),
            ["mode"] = ScalarSchema("mode", ResourceEditorValueKind.Enum, ["Active", "Passive"]),
            ["note"] = ScalarSchema("note", ResourceEditorValueKind.String),
            ["containers"] = new("containers", ResourceEditorValueKind.Array, null, EmptyProperties, container, [], EmptyRequired, "Container list"),
            ["labels"] = new("labels", ResourceEditorValueKind.Map, null, EmptyProperties, ScalarSchema("value", ResourceEditorValueKind.String), [], EmptyRequired, "Labels"),
        });
        return ObjectSchema("root", new Dictionary<string, ResourceEditorSchemaNode> { ["spec"] = spec });
    }

    private static ResourceEditorSchemaNode ObjectSchema(
        string name,
        IReadOnlyDictionary<string, ResourceEditorSchemaNode> properties)
        => new(name, ResourceEditorValueKind.Object, null, properties, null, [], EmptyRequired, null);

    private static ResourceEditorSchemaNode ScalarSchema(
        string name,
        ResourceEditorValueKind kind,
        IReadOnlyList<string>? enumValues = null,
        string? description = null)
        => new(name, kind, null, EmptyProperties, null, enumValues ?? [], EmptyRequired, description);

    private static Func<ExpandableSection, bool> HasTooltip(string description)
        => section => section.Header is Control header && Equals(ToolTip.GetTip(header), description);

    private static bool HasCenteredHeader(ExpandableSection section)
        => section.Header is Grid header
            && header.Children.OfType<StackPanel>().Single().VerticalAlignment == VerticalAlignment.Center;

    private static readonly IReadOnlyDictionary<string, ResourceEditorSchemaNode> EmptyProperties =
        new Dictionary<string, ResourceEditorSchemaNode>(StringComparer.Ordinal);
    private static readonly IReadOnlySet<string> EmptyRequired = new HashSet<string>(StringComparer.Ordinal);
}
