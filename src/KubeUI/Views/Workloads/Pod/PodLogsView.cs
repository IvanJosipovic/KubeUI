using Avalonia.Controls.Primitives;
using Avalonia.Input;
using AvaloniaEdit;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;

namespace KubeUI.Views;

public sealed class PodLogsView : MyViewBase<PodLogsViewModel>
{
    private Installation _textMateInstallation;

    private RegistryOptions _registryOptions;

    protected override object Build(PodLogsViewModel? vm)
    {
        return new Grid()
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .VerticalAlignment(VerticalAlignment.Stretch)
            .Rows("Auto,*")
            .Children([
                new StackPanel()
                    .Row(0)
                    .Orientation(Orientation.Horizontal)
                    .Children([
                        new Label()
                            .Margin(0,0,4,0)
                            .Content("Pod: "),
                        new Label()
                            .Margin(0,0,4,0)
                            .Content(new MultiBinding(){
                                Bindings = [
                                        new Binding("Object.Metadata.NamespaceProperty"),
                                        new Binding("Object.Metadata.Name"),
                                        new Binding(nameof(PodConsoleViewModel.ContainerName))
                                ],
                                StringFormat = "{0}/{1}/{2}"
                            }),
                        new ToggleButton()
                            .IsChecked(@vm.Previous)
                            .ToolTip(Assets.Resources.PodLogsView_Previous)
                            .Content(new PathIcon() { Data = (Geometry)Application.Current.FindResource("arrow_undo_regular") }),
                        new ToggleButton()
                            .IsChecked(@vm.Timestamps)
                            .ToolTip(Assets.Resources.PodLogsView_Timestamps)
                            .Content(new PathIcon() { Data = (Geometry)Application.Current.FindResource("timer_regular") }),
                        new ToggleButton()
                            .IsChecked(@vm.AutoScrollToBottom)
                            .ToolTip(Assets.Resources.PodLogsView_AutoScrollToBottom)
                            .Content(new PathIcon() { Data = (Geometry)Application.Current.FindResource("ic_fluent_padding_down_filled") }),
                    ]),
                new TextEditor()
                    .Ref(out var editor)
                    .Row(1)
                    .Set(x => {
                        _registryOptions = new RegistryOptions(ThemeName.DarkPlus);
                        _textMateInstallation = editor.InstallTextMate(_registryOptions, false);

                        x.Options.AllowScrollBelowDocument = false;
                        x.Options.ShowBoxForControlCharacters = false;
                        x.Options.EnableHyperlinks = false;
                        x.Options.EnableEmailHyperlinks = false;

                        x.TextChanged += (sender, e) => {
                            if (ViewModel.AutoScrollToBottom)
                                editor.ScrollToEnd();
                        };
                        return x;
                    })
                    .Set(TextEditor.DocumentProperty, @vm.Logs, BindingMode.OneWay)
                    .Set(TextEditor.FontFamilyProperty, new FontFamily("Consolas,Menlo,Monospace"))
                    .Set(TextEditor.FontSizeProperty, 14.0)
                    .Set(TextEditor.FontWeightProperty, FontWeight.Normal)
                    .Set(TextEditor.IsReadOnlyProperty, true)
                    .Set(TextEditor.ShowLineNumbersProperty, false)
                    .Background(Brushes.Black)
                    .HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
                    .VerticalScrollBarVisibility(ScrollBarVisibility.Visible)
                    .ContextMenu([
                            new MenuItem()
                                .OnClick((_) => editor.Copy())
                                .Header(Assets.Resources.Action_Copy)
                                .InputGesture(new KeyGesture(Key.C, KeyModifiers.Control))
                                .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("copy_regular") }),
                        ]),
            ]);
    }
}
