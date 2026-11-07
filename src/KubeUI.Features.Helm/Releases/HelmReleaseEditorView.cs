using Avalonia.Automation;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Xaml.Interactions.Core;
using AvaloniaEdit;
using KubeUI.Controls.Presentation;
using KubeUI.Controls.Styles;
using KubeUI.Controls.Editors.Behaviors;

namespace KubeUI.Features.Helm.Releases;

public sealed class HelmReleaseEditorView : KubeUIView<HelmReleaseEditorViewModel>
{
    protected override object Build(HelmReleaseEditorViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        var createNamespaceCheckBox = new CheckBox()
            .Row(6)
            .Col(1)
            .IsChecked(vm, x => x.CreateNamespace, BindingMode.TwoWay);
        AutomationProperties.SetName(createNamespaceCheckBox, HelmLanguageManager.Instance.HelmReleaseEditorView_CreateNamespace.CurrentValue()!);

        var valuesEditor = new TextEditor();
        YamlEditorDefaults.Apply(valuesEditor);
        valuesEditor
            .Name("ValuesYamlEditor", Scope)
            .MinHeight(200)
            .MaxHeight(400)
            .IsReadOnly(vm, x => x.IsLoading)
            .FontFamily(new DynamicResourceExtension(Typography.CodeFontFamilyResourceKey))
            .FontSize(new DynamicResourceExtension(Typography.CodeFontSizeResourceKey))
            .WordWrap(false)
            .BindValue(
                DataValidationErrors.ErrorsProperty,
                CompiledBinding.Create<HelmReleaseEditorViewModel, IEnumerable<object>>(
                    x => x.ValuesYamlValidationErrors));
        Interaction.GetBehaviors(valuesEditor).Add(new HelmValuesEditorBehavior());
        Interaction.GetBehaviors(valuesEditor).Add(new YamlSyntaxHighlightingBehavior());
        Interaction.GetBehaviors(valuesEditor).Add(new YamlFoldingBehavior());

        return new ScrollViewer()
            .HorizontalScrollBarVisibility(ScrollBarVisibility.Disabled)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
            .Content(new Grid()
            .Rows("Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto")
            .Cols("Auto,*")
            .Margin(16)
            .RowSpacing(8)
            .ColumnSpacing(12)
            .Children(
                new TextBlock().Row(0).Col(0).Text(HelmLanguageManager.Instance.HelmReleaseEditorView_Name.ToBinding()),
                new TextBox().Row(0).Col(1).Name("ReleaseName", Scope).IsReadOnly(vm, x => x.IsUpgrade).Text(vm, x => x.ReleaseName, BindingMode.TwoWay),
                new TextBlock().Row(1).Col(0).Text(HelmLanguageManager.Instance.HelmReleaseEditorView_ChartSource.ToBinding()),
                new StackPanel()
                    .Row(1).Col(1)
                    .Orientation(Orientation.Horizontal)
                    .Spacing(12)
                    .Children(
                        new RadioButton()
                            .Content(HelmLanguageManager.Instance.HelmReleaseEditorView_RepositorySource.ToBinding())
                            .GroupName("HelmChartSource")
                            .IsEnabled(vm, x => !x.IsLoading)
                            .IsChecked(vm, x => x.IsRepositorySource, BindingMode.TwoWay),
                        new RadioButton()
                            .Content(HelmLanguageManager.Instance.HelmReleaseEditorView_OciSource.ToBinding())
                            .GroupName("HelmChartSource")
                            .IsEnabled(vm, x => !x.IsLoading)
                            .IsChecked(vm, x => x.IsOciSource, BindingMode.TwoWay)),
                new TextBlock().Row(2).Col(0)
                    .IsVisible(vm, x => x.IsRepositorySource)
                    .Text(HelmLanguageManager.Instance.HelmReleaseEditorView_Repository.ToBinding()),
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
                    .Text(HelmLanguageManager.Instance.HelmReleaseEditorView_Chart.ToBinding()),
                new ComboBox()
                    .Row(3).Col(1)
                    .Name("Chart", Scope)
                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                    .IsEnabled(vm, x => x.CanSelectRepositoryChart)
                    .IsVisible(vm, x => x.IsRepositorySource)
                    .ItemsSource(vm, x => x.AvailableCharts)
                    .SelectedItem(vm, x => x.SelectedChart, BindingMode.TwoWay),
                new TextBlock().Row(3).Col(0)
                    .IsVisible(vm, x => x.IsOciSource)
                    .Text(HelmLanguageManager.Instance.HelmReleaseEditorView_ChartReference.ToBinding()),
                new TextBox().Row(3).Col(1)
                    .Name("ChartReference", Scope)
                    .IsReadOnly(vm, x => x.IsLoading)
                    .IsVisible(vm, x => x.IsOciSource)
                    .Text(vm, x => x.Chart, BindingMode.TwoWay),
                new TextBlock().Row(4).Col(0).Text(HelmLanguageManager.Instance.HelmReleaseEditorView_ChartVersion.ToBinding()),
                new ComboBox()
                    .Row(4).Col(1)
                    .Name("ChartVersion", Scope)
                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                    .IsEnabled(vm, x => x.CanSelectRepositoryChartVersion)
                    .IsVisible(vm, x => x.IsRepositorySource)
                    .ItemsSource(vm, x => x.AvailableVersions)
                    .SelectedItem(vm, x => x.SelectedVersion, BindingMode.TwoWay),
                new TextBox()
                    .Row(4).Col(1)
                    .Name("ChartVersionText", Scope)
                    .IsEnabled(vm, x => x.CanEditOciChartVersion)
                    .IsVisible(vm, x => x.IsOciSource)
                    .Text(vm, x => x.SelectedVersion, BindingMode.TwoWay)
                    .PlaceholderText(HelmLanguageManager.Instance.HelmReleaseEditorView_ChartVersionPlaceholder.ToBinding()),
                new TextBlock().Row(5).Col(0).Text(HelmLanguageManager.Instance.HelmReleaseEditorView_Namespace.ToBinding()),
                new TextBox().Row(5).Col(1).IsReadOnly(vm, x => x.IsUpgrade).Text(vm, x => x.Namespace, BindingMode.TwoWay),
                new TextBlock().Row(6).Col(0).Text(HelmLanguageManager.Instance.HelmReleaseEditorView_CreateNamespace.ToBinding()),
                createNamespaceCheckBox,
                new TextBlock().Row(7).Col(0).Text(HelmLanguageManager.Instance.HelmReleaseEditorView_Timeout.ToBinding()),
                new NumericUpDown().Row(7).Col(1).Minimum(0).Value(vm, x => x.TimeoutSeconds, BindingMode.TwoWay),
                new TextBlock().Row(8).Col(0).Text(HelmLanguageManager.Instance.HelmReleaseEditorView_ValuesYaml.ToBinding()),
                valuesEditor.Row(8).Col(1),
                new CheckBox()
                    .Row(9)
                    .ColSpan(2)
                    .IsVisible(vm, x => x.IsUpgrade)
                    .Content(HelmLanguageManager.Instance.HelmReleaseEditorView_InstallIfMissing.ToBinding())
                    .IsChecked(vm, x => x.InstallIfMissing, BindingMode.TwoWay),
                new StackPanel()
                    .Row(10)
                    .ColSpan(2)
                    .Orientation(Orientation.Horizontal)
                    .HorizontalAlignment(HorizontalAlignment.Right)
                    .Spacing(8)
                    .Children(
                        new Button()
                            .Command(vm, x => x.CancelCommand)
                            .Content(FoundationLanguageManager.Instance.Shared_Cancel.ToBinding()),
                        new Button()
                            .Command(vm, x => x.PreviewCommand)
                            .Content(HelmLanguageManager.Instance.HelmReleaseEditorView_Preview.ToBinding()),
                        new Button()
                            .Command(vm, x => x.SaveCommand)
                            .Content(HelmLanguageManager.Instance.HelmReleaseEditorView_InstallUpgrade.ToBinding()))));
    }
}
