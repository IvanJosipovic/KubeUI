using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.MarkupExtensions;
using FluentAvalonia.UI.Controls;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using SharedConverters = KubeUI.Avalonia.Converters.Converters;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

internal sealed class DataDisplay<TResource, TValue> : UserControl, IInitializeCluster, IResourcePropertiesRefreshable<TResource>
    where TResource : class, IKubernetesObject<V1ObjectMeta>, new()
{
    internal DataDisplay(
        TResource resource,
        GroupApiVersionKind kind,
        Func<TResource, IEnumerable<KeyValuePair<string, TValue>>?> dataSelector,
        Func<TValue, string> displayFormatter,
        Func<string, string> wireFormatter,
        Func<TValue, string>? originalValueWireFormatter = null)
    {
        ViewModel = new DataDisplayViewModel<TResource, TValue>(
            resource,
            kind,
            dataSelector,
            displayFormatter,
            wireFormatter,
            originalValueWireFormatter);
        DataContext = ViewModel;
        Content = CreateContent(ViewModel);
        HorizontalAlignment = HorizontalAlignment.Stretch;
    }

    internal DataDisplayViewModel<TResource, TValue> ViewModel { get; }

    public ClusterWorkspace? Cluster => ViewModel.Cluster;

    public void Initialize(ClusterWorkspace cluster)
    {
        ViewModel.Initialize(cluster);
    }

    public void Refresh(TResource resource)
    {
        ViewModel.Refresh(resource);
    }

    private static Grid CreateContent(DataDisplayViewModel<TResource, TValue> viewModel)
    {
        return new Grid()
            .Rows("Auto,Auto,Auto,*")
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .Children(
                CreateReadOnlyToolbar(viewModel),
                CreateEditToolbar(viewModel),
                new FAInfoBar()
                    .Row(1)
                    .Title(viewModel, x => x.ActionResultTitle)
                    .Margin(4)
                    .CloseButtonCommand(viewModel, x => x.DismissActionResultCommand)
                    .IsClosable(true)
                    .IsOpen(viewModel, x => x.HasActionResult)
                    .IsVisible(viewModel, x => x.HasActionResult)
                    .Message(viewModel, x => x.ActionResultMessage)
                    .Severity(viewModel, x => x.ActionResultSeverity),
                new TextBlock()
                    .Row(2)
                    .Margin(4)
                    .Foreground(new DynamicResourceExtension("SystemErrorTextColor"))
                    .Text(viewModel, x => x.ValidationMessage)
                    .IsVisible(viewModel, x => x.ValidationMessage, BindingMode.OneWay, SharedConverters.HasText),
                new ItemsControl()
                    .Row(3)
                    .ItemsSource(viewModel, x => x.Rows)
                    .ItemTemplate(new FuncDataTemplate<DataDisplayRowViewModel>((row, _) => CreateRow(viewModel, row!))));
    }

    private static StackPanel CreateReadOnlyToolbar(DataDisplayViewModel<TResource, TValue> viewModel)
    {
        return new StackPanel()
            .Orientation(Orientation.Horizontal)
            .IsVisible(viewModel, x => x.EditMode, BindingMode.OneWay, SharedConverters.Not)
            .Children(
                new Button()
                    .Command(viewModel, x => x.BeginEditCommand)
                    .IsVisible(viewModel, x => x.CanEdit)
                    .ToolTip_Tip(Assets.Resources.DataDisplay_Edit)
                    .Content(new FluentIcon().Icon(Icon.DocumentEdit)));
    }

    private static StackPanel CreateEditToolbar(DataDisplayViewModel<TResource, TValue> viewModel)
    {
        return new StackPanel()
            .Orientation(Orientation.Horizontal)
            .IsVisible(viewModel, x => x.EditMode)
            .Children(
                new Button()
                    .Command(viewModel, x => x.SaveCommand)
                    .IsEnabled(viewModel, x => x.CanSave)
                    .ToolTip_Tip(Assets.Resources.DataDisplay_Save)
                    .Content(new FluentIcon().Icon(Icon.Save)),
                new Button()
                    .Command(viewModel, x => x.AddCommand)
                    .ToolTip_Tip(Assets.Resources.DataDisplay_Add)
                    .Content(new FluentIcon().Icon(Icon.Add)),
                new Button()
                    .Command(viewModel, x => x.CancelCommand)
                    .ToolTip_Tip(Assets.Resources.DataDisplay_Cancel)
                    .Content(new FluentIcon().Icon(Icon.Dismiss)));
    }

    private static ExpandableSection CreateRow(
        DataDisplayViewModel<TResource, TValue> viewModel,
        DataDisplayRowViewModel row)
    {
        return new ExpandableSection()
            .Header(CreateRowHeader(viewModel, row))
            .IsExpanded(true)
            .Content(CreateRowContent(viewModel, row));
    }

    private static Grid CreateRowHeader(
        DataDisplayViewModel<TResource, TValue> viewModel,
        DataDisplayRowViewModel row)
    {
        return new Grid()
            .Cols("*,Auto")
            .Children(
                new ScrollViewer()
                    .Col(0)
                    .HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
                    .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
                    .IsVisible(viewModel, x => x.EditMode, BindingMode.OneWay, SharedConverters.Not)
                    .Content(
                        new SelectableTextBlock()
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(row, x => x.Key)
                            .TextWrapping(TextWrapping.NoWrap)
                            .IsVisible(viewModel, x => x.EditMode, BindingMode.OneWay, SharedConverters.Not)),
                CreateEditor(row, editsKey: true)
                    .Col(0)
                    .VerticalContentAlignment(VerticalAlignment.Center)
                    .IsVisible(viewModel, x => x.EditMode),
                new Button()
                    .Col(1)
                    .Command(viewModel, x => x.RemoveCommand)
                    .CommandParameter(row)
                    .IsVisible(viewModel, x => x.EditMode)
                    .ToolTip_Tip(Assets.Resources.DataDisplay_Remove)
                    .Content(new FluentIcon().Icon(Icon.Delete)));
    }

    private static Grid CreateRowContent(
        DataDisplayViewModel<TResource, TValue> viewModel,
        DataDisplayRowViewModel row)
    {
        return new Grid()
            .Children(
                new ScrollViewer()
                    .MaxHeight(200)
                    .HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
                    .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
                    .IsVisible(viewModel, x => x.EditMode, BindingMode.OneWay, SharedConverters.Not)
                    .Content(
                        new SelectableTextBlock()
                            .Padding(5)
                            .Text(row, x => x.Value)
                            .TextWrapping(TextWrapping.NoWrap)
                            .IsVisible(viewModel, x => x.EditMode, BindingMode.OneWay, SharedConverters.Not)),
                CreateEditor(row, editsKey: false)
                    .IsVisible(viewModel, x => x.EditMode));
    }

    private static TextBox CreateEditor(DataDisplayRowViewModel row, bool editsKey)
    {
        var editor = new TextBox()
            .Background(Brushes.Transparent)
            .BorderBrush(Brushes.Transparent)
            .BorderThickness(new Thickness(0))
            .FocusAdorner((ITemplate<Control>?)null)
            .Foreground(new DynamicResourceExtension("TextControlForeground"))
            .MinHeight(0)
            .Padding(editsKey ? new Thickness(0) : new Thickness(5))
            .AcceptsReturn(!editsKey)
            .TextWrapping(TextWrapping.NoWrap)
            .ScrollViewer_HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
            .ScrollViewer_VerticalScrollBarVisibility(ScrollBarVisibility.Auto);

        editor.Resources["TextControlBackgroundFocused"] = Brushes.Transparent;
        editor.Resources["TextControlBackgroundPointerOver"] = Brushes.Transparent;
        editor.Resources["TextControlBorderBrushFocused"] = Brushes.Transparent;
        editor.Resources["TextControlBorderBrushPointerOver"] = Brushes.Transparent;
        editor.Resources["TextControlBorderThemeThicknessFocused"] = new Thickness(0);

        if (editsKey)
        {
            return editor.Text(row, x => x.Key, BindingMode.TwoWay);
        }

        return editor
            .MaxHeight(200)
            .Text(row, x => x.Value, BindingMode.TwoWay);
    }
}
