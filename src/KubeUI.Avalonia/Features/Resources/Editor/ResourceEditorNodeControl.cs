using System.Globalization;
using System.Windows.Input;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using AvaloniaEdit;
using Avalonia.Xaml.Interactivity;
using FluentAvalonia.UI.Controls;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Styles;

namespace KubeUI.Avalonia.Features.Resources.Editor;

public sealed class ResourceEditorNodeControl : UserControl
{
    public ResourceEditorNodeControl(ResourceEditorNodeViewModel node)
    {
        ArgumentNullException.ThrowIfNull(node);
        Node = node;
        DataContext = node;
        Content = node.IsSection ? CreateSection(node) : CreateScalar(node);
    }

    public ResourceEditorNodeViewModel Node { get; }

    private static Control CreateSection(ResourceEditorNodeViewModel node)
    {
        var children = new ItemsControl()
            .ItemTemplate(new FuncDataTemplate<ResourceEditorNodeViewModel>(
                static (child, _) => child is null ? null : new ResourceEditorNodeControl(child)));
        children.Bind(
            ItemsControl.ItemsSourceProperty,
            CompiledBinding.Create<ResourceEditorNodeViewModel, IEnumerable<ResourceEditorNodeViewModel>>(
                x => x.Children));

        var section = new ExpandableSection()
            .Name("EditorSection")
            .Header(CreateHeader(node))
            .Content(new StackPanel()
                .Spacing(2)
                .Margin(new Thickness(8, 2, 0, 2))
                .Children(CreateCollectionToolbar(node), children))
            .IsExpanded(node.IsExpanded);
        section.Bind(
            Expander.IsExpandedProperty,
            CompiledBinding.Create<ResourceEditorNodeViewModel, bool>(
                x => x.IsExpanded,
                mode: BindingMode.TwoWay));
        BindValidation(section, ResourceEditorNodeViewModel.SectionValidationPropertyName);
        var validationBorder = new Border()
            .Name("ValidationErrorOutline")
            .BorderBrush(new DynamicResourceExtension("SystemControlErrorTextForegroundBrush"))
            .CornerRadius(new CornerRadius(2))
            .Child(section);
        validationBorder.Bind(
            Border.BorderThicknessProperty,
            CompiledBinding.Create<ResourceEditorNodeViewModel, bool>(
                x => x.HasErrors,
                converter: ValidationOutlineThicknessConverter.Instance));
        return validationBorder;
    }

    private static Control CreateHeader(ResourceEditorNodeViewModel node)
    {
        var title = new TextBlock()
            .FontWeight(FontWeight.SemiBold)
            .VerticalAlignment(VerticalAlignment.Center);
        title.Bind(
            TextBlock.TextProperty,
            CompiledBinding.Create<ResourceEditorNodeViewModel, string>(x => x.DisplayName));
        var panel = new StackPanel()
            .Orientation(Orientation.Horizontal)
            .Spacing(2)
            .VerticalAlignment(VerticalAlignment.Center)
            .Children(title);

        var header = new Grid().Cols("*,Auto,Auto").ColumnSpacing(4).Children(
            panel,
            CreateClearSectionButton(node).Col(1),
            CreateRemoveButton(node).Col(2));
        if (!string.IsNullOrWhiteSpace(node.Description))
            ToolTip.SetTip(header, node.Description);
        return header;
    }

    private static Control CreateCollectionToolbar(ResourceEditorNodeViewModel node)
    {
        if (node.CanAddItem)
        {
            var add = new Button()
                .Name("AddItemButton")
                .Content(Assets.Resources.ResourceEditorView_AddItem);
            add.Bind(
                Button.CommandProperty,
                CompiledBinding.Create<ResourceEditorNodeViewModel, ICommand?>(x => x.AddItemCommand));
            return add;
        }

        if (node.CanAddMapEntry)
        {
            var key = CreateTextEditor("MapKeyEditor", node.IsReadOnly);
            key.PlaceholderText = Assets.Resources.ResourceEditorView_MapKey;
            key.Bind(
                TextBox.TextProperty,
                CompiledBinding.Create<ResourceEditorNodeViewModel, string>(
                    x => x.NewMapKey,
                    mode: BindingMode.TwoWay,
                    updateSourceTrigger: UpdateSourceTrigger.PropertyChanged));
            var add = new Button()
                .Name("AddMapEntryButton")
                .Content(Assets.Resources.ResourceEditorView_AddEntry);
            add.Bind(
                Button.CommandProperty,
                CompiledBinding.Create<ResourceEditorNodeViewModel, ICommand?>(x => x.AddMapEntryCommand));
            return new Grid().Cols("*,Auto").ColumnSpacing(6).Children(key, add.Col(1));
        }

        return new Border().IsVisible(false);
    }

    private static Control CreateScalar(ResourceEditorNodeViewModel node)
    {
        var label = new StackPanel()
            .Orientation(Orientation.Horizontal)
            .Spacing(6)
            .VerticalAlignment(VerticalAlignment.Center);
        var labelText = new TextBlock().VerticalAlignment(VerticalAlignment.Center);
        labelText.Bind(
            TextBlock.TextProperty,
            CompiledBinding.Create<ResourceEditorNodeViewModel, string>(x => x.DisplayName));
        label.Children.Add(labelText);
        var editor = CreateEditor(node);
        ApplyEditorAutomation(editor, node, labelText);
        if (!string.IsNullOrWhiteSpace(node.Description))
        {
            ToolTip.SetTip(label, node.Description);
            ToolTip.SetTip(editor, node.Description);
        }

        var row = new Grid().Name("ScalarEditor").Margin(4, 0).Cols("280,*,Auto").ColumnSpacing(10).Children(
            label,
            editor.Col(1),
            CreateRemoveButton(node).Col(2));
        return row;
    }

    private static Control CreateEditor(ResourceEditorNodeViewModel node)
    {
        if (node.IsYamlText)
        {
            var editor = new TextEditor()
                .Name("YamlValueEditor")
                .MinHeight(140)
                .ShowLineNumbers(true)
                .IsReadOnly(node.IsReadOnly)
                .WordWrap(true)
                .FontFamily(new FontFamily(Typography.CodeFontFamilyName))
                .FontSize(Typography.DefaultCodeFontSize)
                .FontWeight(FontWeight.Normal)
                .HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
                .VerticalScrollBarVisibility(ScrollBarVisibility.Visible)
                .Background(ApplicationBrushResources.GetBrush("SystemAltHighColor"));
            editor.Text = node.YamlValue;
            BindValidation(editor, nameof(ResourceEditorNodeViewModel.YamlValue));
            Interaction.GetBehaviors(editor).Add(new ResourceEditorYamlTextBehavior());
            return editor;
        }

        if (node.IsReadOnly)
        {
            var editor = new SelectableTextBlock()
                .Name("ReadOnlyEditor")
                .VerticalAlignment(VerticalAlignment.Center)
                .TextWrapping(TextWrapping.NoWrap);
            BindValidation(editor, node.ValidationPropertyName);
            editor.Bind(
                SelectableTextBlock.TextProperty,
                CompiledBinding.Create<ResourceEditorNodeViewModel, string>(x => x.DisplayValue));
            return editor;
        }

        if (node.Kind == ResourceEditorValueKind.Boolean)
        {
            var editor = new CheckBox()
                .Name("BooleanEditor")
                .IsEnabled(!node.IsReadOnly);
            editor.Bind(
                ToggleButton.IsCheckedProperty,
                CompiledBinding.Create<ResourceEditorNodeViewModel, bool>(
                    x => x.BooleanValue,
                    mode: BindingMode.TwoWay));
            return editor;
        }

        if (node.Kind == ResourceEditorValueKind.Enum)
        {
            var editor = new ComboBox()
                .Name("EnumEditor")
                .ItemsSource(node.EnumOptions)
                .IsEnabled(!node.IsReadOnly)
                .ItemTemplate(new FuncDataTemplate<ResourceEditorEnumOption>(static (option, _) =>
                    option is null ? null : new TextBlock().Text(option.DisplayName)));
            editor.Bind(
                SelectingItemsControl.SelectedItemProperty,
                CompiledBinding.Create<ResourceEditorNodeViewModel, ResourceEditorEnumOption?>(
                    x => x.SelectedEnumOption,
                    mode: BindingMode.TwoWay));
            return editor;
        }

        var text = CreateTextEditor(
            node.Kind == ResourceEditorValueKind.Number ? "NumberEditor" : "StringEditor",
            node.IsReadOnly);
        text.AcceptsReturn = node.Kind == ResourceEditorValueKind.Unknown;
        text.TextWrapping = TextWrapping.Wrap;
        if (node.Kind == ResourceEditorValueKind.Number)
        {
            text.Bind(
                TextBox.TextProperty,
                CompiledBinding.Create<ResourceEditorNodeViewModel, string>(
                    x => x.NumberValue,
                    mode: BindingMode.TwoWay,
                    updateSourceTrigger: UpdateSourceTrigger.PropertyChanged));
        }
        else
        {
            text.Bind(
                TextBox.TextProperty,
                CompiledBinding.Create<ResourceEditorNodeViewModel, string>(
                    x => x.StringValue,
                    mode: BindingMode.TwoWay,
                    updateSourceTrigger: UpdateSourceTrigger.PropertyChanged));
        }
        return text;
    }

    private static TextBox CreateTextEditor(string name, bool isReadOnly)
    {
        return new TextBox()
            .Name(name)
            .IsReadOnly(isReadOnly)
            .Opacity(isReadOnly ? 0.92 : 1)
            .VerticalContentAlignment(VerticalAlignment.Center)
            .Background(ApplicationBrushResources.GetBrush(isReadOnly
                ? "SystemControlBackgroundBaseLowBrush"
                : "SystemRegionBrush"))
            .BorderBrush(ApplicationBrushResources.GetBrush("SystemControlForegroundBaseMediumLowBrush"))
            .BorderThickness(new Thickness(1));
    }

    private static Button CreateRemoveButton(ResourceEditorNodeViewModel node)
    {
        var remove = new Button()
            .Name("RemoveButton")
            .Content(Assets.Resources.ResourceEditorView_Remove)
            .IsVisible(node.CanRemove);
        remove.Bind(
            Button.CommandProperty,
            CompiledBinding.Create<ResourceEditorNodeViewModel, ICommand?>(x => x.RemoveCommand));
        return remove;
    }

    private static Button CreateClearSectionButton(ResourceEditorNodeViewModel node)
    {
        var clear = new Button()
            .Name("DeleteSectionButton")
            .Content(new FluentIcon().Icon(Icon.Delete))
            .IsVisible(node.CanClearSection);
        ToolTip.SetTip(clear, Assets.Resources.ResourceEditorView_DeleteSection);
        AutomationProperties.SetName(clear, Assets.Resources.ResourceEditorView_DeleteSection);
        clear.Bind(
            Button.CommandProperty,
            CompiledBinding.Create<ResourceEditorNodeViewModel, ICommand?>(x => x.ClearSectionCommand));
        return clear;
    }

    private static void BindValidation(Control control, string propertyName)
    {
        control.Bind(
            DataValidationErrors.ErrorsProperty,
            CompiledBinding.Create<ResourceEditorNodeViewModel, IReadOnlyDictionary<string, IReadOnlyList<string>>>(
                x => x.ValidationErrorsByProperty,
                converter: ValidationErrorsConverter.Instance,
                mode: BindingMode.OneWay,
                converterParameter: propertyName));
    }

    private static void ApplyEditorAutomation(Control editor, ResourceEditorNodeViewModel node, TextBlock label)
    {
        AutomationProperties.SetAutomationId(editor, $"ResourceEditor_{node.Path}");
        AutomationProperties.SetName(editor, node.DisplayName.TrimEnd(' ', '*'));
        AutomationProperties.SetLabeledBy(editor, label);
    }

    private sealed class ValidationErrorsConverter : IValueConverter
    {
        public static ValidationErrorsConverter Instance { get; } = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is IReadOnlyDictionary<string, IReadOnlyList<string>> errors
                && parameter is string propertyName
                && errors.TryGetValue(propertyName, out var messages)
                ? messages
                : Array.Empty<string>();

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    private sealed class ValidationOutlineThicknessConverter : IValueConverter
    {
        public static ValidationOutlineThicknessConverter Instance { get; } = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is true ? new Thickness(1) : new Thickness(0);

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
