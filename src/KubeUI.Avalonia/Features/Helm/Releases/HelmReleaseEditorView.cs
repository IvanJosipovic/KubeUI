using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Controls.Primitives;
using Avalonia.Xaml.Interactions.Core;
using AvaloniaEdit;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Styles;

namespace KubeUI.Avalonia.Features.Helm.Releases;

public sealed class HelmReleaseEditorView : ViewBase<HelmReleaseEditorViewModel>
{
    protected override object Build(HelmReleaseEditorViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new Grid()
            .Rows("Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto")
            .Cols("Auto,*")
            .Margin(16)
            .RowSpacing(8)
            .ColumnSpacing(12)
            .Children(
                new TextBlock().Row(0).Col(0).Text(Assets.Resources.HelmReleaseEditorView_Name),
                new TextBox().Row(0).Col(1).Text(vm, x => x.ReleaseName, BindingMode.TwoWay),
                new TextBlock().Row(1).Col(0).Text(Assets.Resources.HelmReleaseEditorView_Chart),
                new TextBox().Row(1).Col(1).Text(vm, x => x.Chart, BindingMode.TwoWay),
                new TextBlock().Row(2).Col(0).Text(Assets.Resources.HelmReleaseEditorView_Namespace),
                new TextBox().Row(2).Col(1).Text(vm, x => x.Namespace, BindingMode.TwoWay),
                new TextBlock().Row(3).Col(0).Text(Assets.Resources.HelmReleaseEditorView_ValuesJson),
                new TextEditor()
                    .Row(3)
                    .Col(1)
                    .MinHeight(200)
                    .FontFamily(new DynamicResourceExtension(Typography.CodeFontFamilyResourceKey))
                    .FontSize(new DynamicResourceExtension(Typography.CodeFontSizeResourceKey))
                    .ShowLineNumbers(true)
                    .HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
                    .VerticalScrollBarVisibility(ScrollBarVisibility.Visible)
                    .Behaviors(new HelmValuesEditorBehavior()),
                new TextBlock().Row(4).Col(0).Text(Assets.Resources.HelmReleaseEditorView_Timeout),
                new NumericUpDown().Row(4).Col(1).Minimum(0).Value(vm, x => x.TimeoutSeconds, BindingMode.TwoWay),
                new CheckBox()
                    .Row(5)
                    .ColSpan(2)
                    .Content(Assets.Resources.HelmReleaseEditorView_CreateNamespace)
                    .IsChecked(vm, x => x.CreateNamespace, BindingMode.TwoWay),
                new CheckBox()
                    .Row(6)
                    .ColSpan(2)
                    .Content(Assets.Resources.HelmReleaseEditorView_Wait)
                    .IsChecked(vm, x => x.WaitForReady, BindingMode.TwoWay),
                new CheckBox()
                    .Row(7)
                    .ColSpan(2)
                    .IsVisible(vm, x => x.IsUpgrade)
                    .Content(Assets.Resources.HelmReleaseEditorView_InstallIfMissing)
                    .IsChecked(vm, x => x.InstallIfMissing, BindingMode.TwoWay),
                new TextBlock()
                    .Row(8)
                    .ColSpan(2)
                    .Foreground(Brushes.Firebrick)
                    .Text(vm, x => x.Error),
                new StackPanel()
                    .Row(9)
                    .ColSpan(2)
                    .Orientation(Orientation.Horizontal)
                    .HorizontalAlignment(HorizontalAlignment.Right)
                    .Spacing(8)
                    .Children(
                        new Button()
                            .Command(vm, x => x.CancelCommand)
                            .Content(Assets.Resources.HelmReleaseEditorView_Cancel),
                        new Button()
                            .Command(vm, x => x.PreviewCommand)
                            .Content(Assets.Resources.HelmReleaseEditorView_Preview),
                        new Button()
                            .Command(vm, x => x.SaveCommand)
                            .Content(Assets.Resources.HelmReleaseEditorView_InstallUpgrade)));
    }
}
