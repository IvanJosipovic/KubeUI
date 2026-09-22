using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using AvaloniaEdit;
using Avalonia.Xaml.Interactivity;
using FluentAvalonia.UI.Controls;
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
        var children = new ItemsControl
        {
            ItemTemplate = new FuncDataTemplate<ResourceEditorNodeViewModel>(
                static (child, _) => child is null ? null : new ResourceEditorNodeControl(child))
        };
        children.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(ResourceEditorNodeViewModel.Children)));

        var section = new ExpandableSection
        {
            Name = "EditorSection",
            Header = CreateHeader(node),
            Content = new StackPanel
            {
                Spacing = 2,
                Margin = new Thickness(8, 2, 0, 2),
                Children = { CreateCollectionToolbar(node), children }
            },
            IsExpanded = string.IsNullOrEmpty(node.Path) || !node.Path.Contains('.', StringComparison.Ordinal),
        };
        return CreateValidationBorder(section);
    }

    private static Control CreateHeader(ResourceEditorNodeViewModel node)
    {
        var title = new TextBlock
        {
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
        };
        title.Bind(TextBlock.TextProperty, new Binding(nameof(ResourceEditorNodeViewModel.DisplayName)));
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { title }
        };

        var header = new Grid().Cols("*,Auto").Children(panel, CreateRemoveButton(node).Col(1));
        if (!string.IsNullOrWhiteSpace(node.Description))
            ToolTip.SetTip(header, node.Description);
        return header;
    }

    private static Control CreateCollectionToolbar(ResourceEditorNodeViewModel node)
    {
        if (node.CanAddItem)
        {
            var add = new Button { Name = "AddItemButton", Content = Assets.Resources.ResourceEditorView_AddItem };
            add.Bind(Button.CommandProperty, new Binding(nameof(ResourceEditorNodeViewModel.AddItemCommand)));
            return add;
        }

        if (node.CanAddMapEntry)
        {
            var key = CreateTextEditor("MapKeyEditor", node.IsReadOnly);
            key.PlaceholderText = Assets.Resources.ResourceEditorView_MapKey;
            key.Bind(TextBox.TextProperty, new Binding(nameof(ResourceEditorNodeViewModel.NewMapKey))
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            });
            var add = new Button { Name = "AddMapEntryButton", Content = Assets.Resources.ResourceEditorView_AddEntry };
            add.Bind(Button.CommandProperty, new Binding(nameof(ResourceEditorNodeViewModel.AddMapEntryCommand)));
            return new Grid().Cols("*,Auto").ColumnSpacing(6).Children(key, add.Col(1));
        }

        return new Border { IsVisible = false };
    }

    private static Control CreateScalar(ResourceEditorNodeViewModel node)
    {
        var label = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var labelText = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
        labelText.Bind(TextBlock.TextProperty, new Binding(nameof(ResourceEditorNodeViewModel.DisplayName)));
        label.Children.Add(labelText);
        var validation = new TextBlock { Classes = { "error" }, TextWrapping = TextWrapping.Wrap };
        validation.Bind(TextBlock.TextProperty, new Binding(nameof(ResourceEditorNodeViewModel.ValidationMessage)));
        var editor = CreateEditor(node);
        if (!string.IsNullOrWhiteSpace(node.Description))
        {
            ToolTip.SetTip(label, node.Description);
            ToolTip.SetTip(editor, node.Description);
        }

        var row = new Grid().Name("ScalarEditor").Margin(4, 0).Cols("280,*,Auto").Rows("Auto,Auto").ColumnSpacing(10).Children(
            label,
            editor.Col(1),
            CreateRemoveButton(node).Col(2),
            validation.Col(1).Row(1));
        return CreateValidationBorder(row);
    }

    private static Control CreateValidationBorder(Control content)
    {
        var border = new Border
        {
            Child = content,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2),
        };
        border.Bind(Border.BorderBrushProperty, new Binding(nameof(ResourceEditorNodeViewModel.HasValidationError))
        {
            Converter = ValidationBrushConverter.Instance,
        });
        return border;
    }

    private static Control CreateEditor(ResourceEditorNodeViewModel node)
    {
        if (node.IsYamlText)
        {
            var editor = new TextEditor
            {
                Name = "YamlValueEditor",
                MinHeight = 140,
                ShowLineNumbers = true,
                IsReadOnly = node.IsReadOnly,
                WordWrap = true,
                Text = node.YamlValue,
                FontFamily = new FontFamily(Typography.CodeFontFamilyName),
                FontSize = Typography.DefaultCodeFontSize,
                FontWeight = FontWeight.Normal,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                Background = ApplicationBrushResources.GetBrush("SystemAltHighColor"),
            };
            Interaction.GetBehaviors(editor).Add(new ResourceEditorYamlTextBehavior());
            return editor;
        }

        if (node.IsReadOnly)
        {
            var editor = new SelectableTextBlock
            {
                Name = "ReadOnlyEditor",
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
            };
            editor.Bind(SelectableTextBlock.TextProperty, new Binding(nameof(ResourceEditorNodeViewModel.DisplayValue)));
            return editor;
        }

        if (node.Kind == ResourceEditorValueKind.Boolean)
        {
            var editor = new CheckBox { Name = "BooleanEditor", IsEnabled = !node.IsReadOnly };
            editor.Bind(ToggleButton.IsCheckedProperty, new Binding(nameof(ResourceEditorNodeViewModel.BooleanValue)) { Mode = BindingMode.TwoWay });
            return editor;
        }

        if (node.Kind == ResourceEditorValueKind.Enum)
        {
            var editor = new ComboBox { Name = "EnumEditor", ItemsSource = node.EnumOptions, IsEnabled = !node.IsReadOnly };
            editor.ItemTemplate = new FuncDataTemplate<ResourceEditorEnumOption>(static (option, _) =>
                option is null ? null : new TextBlock { Text = option.DisplayName });
            editor.Bind(SelectingItemsControl.SelectedItemProperty, new Binding(nameof(ResourceEditorNodeViewModel.SelectedEnumOption)) { Mode = BindingMode.TwoWay });
            return editor;
        }

        var text = CreateTextEditor(
            node.Kind == ResourceEditorValueKind.Number ? "NumberEditor" : "StringEditor",
            node.IsReadOnly);
        text.AcceptsReturn = node.Kind == ResourceEditorValueKind.Unknown;
        text.TextWrapping = TextWrapping.Wrap;
        text.Bind(TextBox.TextProperty, new Binding(node.Kind == ResourceEditorValueKind.Number
            ? nameof(ResourceEditorNodeViewModel.NumberValue)
            : nameof(ResourceEditorNodeViewModel.StringValue))
        {
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });
        return text;
    }

    private static TextBox CreateTextEditor(string name, bool isReadOnly)
    {
        var text = new TextBox
        {
            Name = name,
            IsReadOnly = isReadOnly,
            Opacity = isReadOnly ? 0.92 : 1,
            VerticalContentAlignment = VerticalAlignment.Center,
            Background = ApplicationBrushResources.GetBrush(isReadOnly
                ? "SystemControlBackgroundBaseLowBrush"
                : "SystemRegionBrush"),
            BorderBrush = ApplicationBrushResources.GetBrush("SystemControlForegroundBaseMediumLowBrush"),
            BorderThickness = new Thickness(1),
        };
        return text;
    }

    private static Button CreateRemoveButton(ResourceEditorNodeViewModel node)
    {
        var remove = new Button
        {
            Name = "RemoveButton",
            Content = Assets.Resources.ResourceEditorView_Remove,
            IsVisible = node.CanRemove,
        };
        remove.Bind(Button.CommandProperty, new Binding(nameof(ResourceEditorNodeViewModel.RemoveCommand)));
        return remove;
    }

    private sealed class ValidationBrushConverter : IValueConverter
    {
        public static ValidationBrushConverter Instance { get; } = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is true
                ? ApplicationBrushResources.GetBrush("ContainerStatusErrorBrush")
                : Brushes.Transparent;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
