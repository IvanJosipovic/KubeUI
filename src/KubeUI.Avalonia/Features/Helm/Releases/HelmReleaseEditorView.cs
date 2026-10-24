using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Xaml.Interactions.Core;
using AvaloniaEdit;
using KubeUI.Avalonia.Features.Resources.Yaml.Behaviors;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Styles;

namespace KubeUI.Avalonia.Features.Helm.Releases;

public sealed class HelmReleaseEditorView : ViewBase<HelmReleaseEditorViewModel>
{
    protected override object Build(HelmReleaseEditorViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new ScrollViewer()
            .HorizontalScrollBarVisibility(ScrollBarVisibility.Disabled)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
            .Content(new Grid()
            .Rows("Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto")
            .Cols("Auto,*")
            .Margin(16)
            .RowSpacing(8)
            .ColumnSpacing(12)
            .Children(
                new TextBlock().Row(0).Col(0).Text(Assets.Resources.HelmReleaseEditorView_Name),
                new TextBox().Row(0).Col(1).IsReadOnly(vm, x => x.IsUpgrade).Text(vm, x => x.ReleaseName, BindingMode.TwoWay),
                new TextBlock().Row(1).Col(0).Text(Assets.Resources.HelmReleaseEditorView_ChartSource),
                new StackPanel()
                    .Row(1).Col(1)
                    .Orientation(Orientation.Horizontal)
                    .Spacing(12)
                    .Children(
                        new RadioButton()
                            .Content(Assets.Resources.HelmReleaseEditorView_RepositorySource)
                            .GroupName("HelmChartSource")
                            .IsEnabled(vm, x => !x.IsLoading)
                            .IsChecked(vm, x => x.IsRepositorySource, BindingMode.TwoWay),
                        new RadioButton()
                            .Content(Assets.Resources.HelmReleaseEditorView_OciSource)
                            .GroupName("HelmChartSource")
                            .IsEnabled(vm, x => !x.IsLoading)
                            .IsChecked(vm, x => x.IsOciSource, BindingMode.TwoWay)),
                new TextBlock().Row(2).Col(0)
                    .IsVisible(vm, x => x.IsRepositorySource)
                    .Text(Assets.Resources.HelmReleaseEditorView_Repository),
                new ComboBox()
                    .Row(2).Col(1)
                    .Name("Repository", Scope)
                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                    .IsEnabled(vm, x => !x.IsLoading)
                    .IsVisible(vm, x => x.IsRepositorySource)
                    .ItemsSource(vm, x => x.Repositories)
                    .SelectedItem(vm, x => x.SelectedRepository, BindingMode.TwoWay),
                new TextBlock().Row(3).Col(0)
                    .IsVisible(vm, x => x.IsRepositorySource)
                    .Text(Assets.Resources.HelmReleaseEditorView_Chart),
                new ComboBox()
                    .Row(3).Col(1)
                    .Name("Chart", Scope)
                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                    .IsEnabled(vm, x => x.CanSelectRepositoryChart)
                    .IsVisible(vm, x => x.IsRepositorySource)
                    .ItemsSource(vm, x => x.AvailableCharts)
                    .SelectedItem(vm, x => x.SelectedChart, BindingMode.TwoWay),
                new TextBlock().Row(4).Col(0)
                    .IsVisible(vm, x => x.IsOciSource)
                    .Text(Assets.Resources.HelmReleaseEditorView_ChartReference),
                new TextBox().Row(4).Col(1)
                    .Name("ChartReference", Scope)
                    .IsReadOnly(vm, x => x.IsLoading)
                    .IsVisible(vm, x => x.IsOciSource)
                    .Text(vm, x => x.Chart, BindingMode.TwoWay),
                new TextBlock().Row(5).Col(0).Text(Assets.Resources.HelmReleaseEditorView_ChartVersion),
                new ComboBox()
                    .Row(5).Col(1)
                    .Name("ChartVersion", Scope)
                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                    .IsEnabled(vm, x => x.CanSelectRepositoryChartVersion)
                    .IsVisible(vm, x => x.IsRepositorySource)
                    .ItemsSource(vm, x => x.AvailableVersions)
                    .SelectedItem(vm, x => x.SelectedVersion, BindingMode.TwoWay)
                    .PlaceholderText(Assets.Resources.HelmReleaseEditorView_ChartVersionPlaceholder),
                new TextBox()
                    .Row(5).Col(1)
                    .Name("ChartVersionText", Scope)
                    .IsEnabled(vm, x => x.CanEditOciChartVersion)
                    .IsVisible(vm, x => x.IsOciSource)
                    .Text(vm, x => x.SelectedVersion, BindingMode.TwoWay)
                    .PlaceholderText(Assets.Resources.HelmReleaseEditorView_ChartVersionPlaceholder),
                new TextBlock().Row(6).Col(0).Text(Assets.Resources.HelmReleaseEditorView_Namespace),
                new TextBox().Row(6).Col(1).IsReadOnly(vm, x => x.IsUpgrade).Text(vm, x => x.Namespace, BindingMode.TwoWay),
                new TextBlock().Row(7).Col(0).Text(Assets.Resources.HelmReleaseEditorView_ValuesYaml),
                new TextEditor()
                    .Row(7)
                    .Col(1)
                    .MinHeight(200)
                    .MaxHeight(400)
                    .IsReadOnly(vm, x => x.IsLoading)
                    .FontFamily(new DynamicResourceExtension(Typography.CodeFontFamilyResourceKey))
                    .FontSize(new DynamicResourceExtension(Typography.CodeFontSizeResourceKey))
                    .ShowLineNumbers(true)
                    .HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
                    .VerticalScrollBarVisibility(ScrollBarVisibility.Visible)
                    .Behaviors(new HelmValuesEditorBehavior(), new YamlSyntaxHighlightingBehavior()),
                new TextBlock().Row(8).Col(0).Text(Assets.Resources.HelmReleaseEditorView_Timeout),
                new NumericUpDown().Row(8).Col(1).Minimum(0).Value(vm, x => x.TimeoutSeconds, BindingMode.TwoWay),
                new CheckBox()
                    .Row(9)
                    .ColSpan(2)
                    .Content(Assets.Resources.HelmReleaseEditorView_CreateNamespace)
                    .IsChecked(vm, x => x.CreateNamespace, BindingMode.TwoWay),
                new CheckBox()
                    .Row(10)
                    .ColSpan(2)
                    .Content(Assets.Resources.HelmReleaseEditorView_Wait)
                    .IsChecked(vm, x => x.WaitForReady, BindingMode.TwoWay),
                new CheckBox()
                    .Row(11)
                    .ColSpan(2)
                    .IsVisible(vm, x => x.IsUpgrade)
                    .Content(Assets.Resources.HelmReleaseEditorView_InstallIfMissing)
                    .IsChecked(vm, x => x.InstallIfMissing, BindingMode.TwoWay),
                new TextBlock()
                    .Row(12)
                    .ColSpan(2)
                    .Foreground(Brushes.Firebrick)
                    .Text(vm, x => x.Error),
                new StackPanel()
                    .Row(13)
                    .ColSpan(2)
                    .Orientation(Orientation.Horizontal)
                    .HorizontalAlignment(HorizontalAlignment.Right)
                    .Spacing(8)
                    .Children(
                        new Button()
                            .Command(vm, x => x.CancelCommand)
                            .Content(Assets.Resources.Shared_Cancel),
                        new Button()
                            .Command(vm, x => x.PreviewCommand)
                            .Content(Assets.Resources.HelmReleaseEditorView_Preview),
                        new Button()
                            .Command(vm, x => x.SaveCommand)
                            .Content(Assets.Resources.HelmReleaseEditorView_InstallUpgrade))));
    }
}
