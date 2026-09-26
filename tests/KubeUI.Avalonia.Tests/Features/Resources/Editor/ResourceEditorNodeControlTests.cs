using System.Text.Json.Nodes;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using FluentIcons.Avalonia;
using FluentIcons.Common;
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
        descendants.OfType<Button>().Where(button => button.Name == "DeleteSectionButton").Count().ShouldBe(4);
        descendants.OfType<Button>().Where(button => button.Name == "DeleteSectionButton" && button.IsVisible)
            .Count().ShouldBe(3);
        var numberEditor = descendants.OfType<TextBox>().Single(editor => editor.Name == "NumberEditor");
        AutomationProperties.GetAutomationId(numberEditor).ShouldBe("ResourceEditor_spec.replicas");
        AutomationProperties.GetName(numberEditor).ShouldBe("replicas");
        AutomationProperties.GetLabeledBy(numberEditor).ShouldNotBeNull();
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
    public void Clearing_section_sets_object_to_null_and_collapses_until_reopened()
    {
        var document = JsonNode.Parse("""{"spec":{"replicas":2,"containers":[],"labels":{}}}""")!;
        var root = ResourceEditorNodeViewModel.Create(CreateSchema(), document);
        var control = new ResourceEditorNodeControl(root);
        using var window = Application.Current.CreateTestWindow(900, 700, control);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var spec = root.Children.Single(node => node.Name == "spec");
        var specControl = FindNodeControl(control, spec);
        var specSection = specControl.Content.ShouldBeOfType<Border>().Child.ShouldBeOfType<ExpandableSection>();
        var clearButton = specSection.Header.ShouldBeOfType<Grid>().GetVisualDescendants().OfType<Button>()
            .Single(button => button.Name == "DeleteSectionButton");
        clearButton.Content.ShouldBeOfType<FluentIcon>().Icon.ShouldBe(Icon.Delete);
        ToolTip.GetTip(clearButton).ShouldBe("Delete");
        AutomationProperties.GetName(clearButton).ShouldBe("Delete");

        var clickPoint = clearButton.TranslatePoint(
            new Point(clearButton.Bounds.Width / 2, clearButton.Bounds.Height / 2),
            window);
        clickPoint.ShouldNotBeNull();
        window.MouseDown(clickPoint!.Value, MouseButton.Left);
        window.MouseUp(clickPoint.Value, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        document["spec"].ShouldBeNull();
        specSection.IsExpanded.ShouldBeFalse();

        specSection.IsExpanded = true;
        Dispatcher.UIThread.RunJobs();
        spec.Children.Single(node => node.Name == "replicas").NumberValue = "3";

        document["spec"]!["replicas"]!.GetValue<decimal>().ShouldBe(3m);
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

    [AvaloniaFact]
    public void Validation_errors_are_attached_to_actual_editors()
    {
        var schema = ObjectSchema("root", new Dictionary<string, ResourceEditorSchemaNode>
        {
            ["replicas"] = ScalarSchema("replicas", ResourceEditorValueKind.Number),
            ["mode"] = ScalarSchema("mode", ResourceEditorValueKind.Enum, ["Active", "Passive"]),
            ["values"] = ScalarSchema("values", ResourceEditorValueKind.String, isYamlEditor: true),
            ["labels"] = new(
                "labels",
                ResourceEditorValueKind.Map,
                null,
                EmptyProperties,
                ScalarSchema("value", ResourceEditorValueKind.String),
                [],
                EmptyRequired,
                null),
        });
        var root = ResourceEditorNodeViewModel.Create(schema, JsonNode.Parse("""{"replicas":2,"mode":"Active","values":"ready","labels":{"team":"platform"}}""")!);
        var control = new ResourceEditorNodeControl(root);
        using var window = Application.Current.CreateTestWindow(900, 700, control);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var replicas = root.Children.Single(node => node.Name == "replicas");
        replicas.NumberValue = "invalid";
        var mode = root.Children.Single(node => node.Name == "mode");
        mode.SelectedEnum = "Invalid";
        var values = root.Children.Single(node => node.Name == "values");
        values.YamlValue = "allow: [";
        var labels = root.Children.Single(node => node.Name == "labels");
        labels.TryAddMapEntry("team").ShouldBeFalse();
        Dispatcher.UIThread.RunJobs();

        var numberEditor = FindNodeControl(control, replicas).GetVisualDescendants().OfType<TextBox>().Single();
        var enumEditor = FindNodeControl(control, mode).GetVisualDescendants().OfType<ComboBox>().Single();
        var yamlEditor = FindNodeControl(control, values).GetVisualDescendants().OfType<TextEditor>().Single();
        var mapKeyEditor = control.GetVisualDescendants().OfType<TextBox>().Single(editor => editor.Name == "MapKeyEditor");

        DataValidationErrors.GetHasErrors(numberEditor).ShouldBeTrue();
        DataValidationErrors.GetHasErrors(enumEditor).ShouldBeTrue();
        DataValidationErrors.GetHasErrors(yamlEditor).ShouldBeTrue();
        DataValidationErrors.GetHasErrors(mapKeyEditor).ShouldBeTrue();

        var labelsSection = FindNodeControl(control, labels).Content.ShouldBeOfType<Border>()
            .Child.ShouldBeOfType<ExpandableSection>();
        DataValidationErrors.GetHasErrors(labelsSection).ShouldBeTrue();
        DataValidationErrors.GetErrors(labelsSection).ShouldContain("labels: Key already exists.");
    }

    [AvaloniaFact]
    public void Validation_error_marks_each_parent_section_invalid_and_clears_when_fixed()
    {
        var root = ResourceEditorNodeViewModel.Create(
            CreateSchema(),
            JsonNode.Parse("""{"spec":{"replicas":2}}""")!);
        var control = new ResourceEditorNodeControl(root);
        using var window = Application.Current.CreateTestWindow(900, 700, control);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var spec = root.Children.Single(node => node.Name == "spec");
        var replicas = spec.Children.Single(node => node.Name == "replicas");
        var specControl = FindNodeControl(control, spec);
        var rootOutline = control.Content.ShouldBeOfType<Border>();
        var specOutline = specControl.Content.ShouldBeOfType<Border>();
        var rootSection = rootOutline.Child.ShouldBeOfType<ExpandableSection>();
        var specSection = specOutline.Child.ShouldBeOfType<ExpandableSection>();
        var numberEditor = FindNodeControl(control, replicas).GetVisualDescendants().OfType<TextBox>().Single();

        rootOutline.IsVisible.ShouldBeTrue();
        specOutline.IsVisible.ShouldBeTrue();
        rootOutline.BorderThickness.ShouldBe(new Thickness(0));
        specOutline.BorderThickness.ShouldBe(new Thickness(0));

        replicas.NumberValue = "invalid";
        Dispatcher.UIThread.RunJobs();

        DataValidationErrors.GetHasErrors(numberEditor).ShouldBeTrue();
        DataValidationErrors.GetHasErrors(specSection).ShouldBeTrue();
        DataValidationErrors.GetHasErrors(rootSection).ShouldBeTrue();
        specOutline.BorderThickness.ShouldBe(new Thickness(1));
        rootOutline.BorderThickness.ShouldBe(new Thickness(1));
        Application.Current!.TryGetResource(
            "SystemControlErrorTextForegroundBrush",
            Application.Current.ActualThemeVariant,
            out var errorBrush).ShouldBeTrue();
        specOutline.BorderBrush.ShouldBe(errorBrush);
        replicas.HasErrors.ShouldBeTrue();
        spec.HasErrors.ShouldBeTrue();
        root.HasErrors.ShouldBeTrue();

        replicas.NumberValue = "3";
        Dispatcher.UIThread.RunJobs();

        DataValidationErrors.GetHasErrors(numberEditor).ShouldBeFalse();
        DataValidationErrors.GetHasErrors(specSection).ShouldBeFalse();
        DataValidationErrors.GetHasErrors(rootSection).ShouldBeFalse();
        specOutline.BorderThickness.ShouldBe(new Thickness(0));
        rootOutline.BorderThickness.ShouldBe(new Thickness(0));
        replicas.HasErrors.ShouldBeFalse();
        spec.HasErrors.ShouldBeFalse();
        root.HasErrors.ShouldBeFalse();
    }

    private static ResourceEditorNodeControl FindNodeControl(
        ResourceEditorNodeControl root,
        ResourceEditorNodeViewModel node)
        => root.GetVisualDescendants().OfType<ResourceEditorNodeControl>()
            .Single(control => ReferenceEquals(control.Node, node));

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
        string? description = null,
        bool isYamlEditor = false)
        => new(name, kind, null, EmptyProperties, null, enumValues ?? [], EmptyRequired, description, isYamlEditor);

    private static Func<ExpandableSection, bool> HasTooltip(string description)
        => section => section.Header is Control header && Equals(ToolTip.GetTip(header), description);

    private static bool HasCenteredHeader(ExpandableSection section)
        => section.Header is Grid header
            && header.GetVisualDescendants().OfType<StackPanel>().Single().VerticalAlignment == VerticalAlignment.Center;

    private static readonly IReadOnlyDictionary<string, ResourceEditorSchemaNode> EmptyProperties =
        new Dictionary<string, ResourceEditorSchemaNode>(StringComparer.Ordinal);
    private static readonly IReadOnlySet<string> EmptyRequired = new HashSet<string>(StringComparer.Ordinal);
}
