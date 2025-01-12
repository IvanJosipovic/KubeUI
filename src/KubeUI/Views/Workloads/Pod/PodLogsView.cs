using System.Reflection;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Styling;
using AvaloniaEdit;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;

namespace KubeUI.Views;

public sealed class PodLogsView : MyViewBase<PodLogsViewModel>
{
    private Installation _textMateInstallation;

    private RegistryOptions _registryOptions;

    private TextEditor _textEditor;

    public PodLogsView()
    {
        _registryOptions = new RegistryOptions(Application.Current.ActualThemeVariant == ThemeVariant.Light ? ThemeName.Light : ThemeName.DarkPlus);

        Application.Current.ActualThemeVariantChanged += Current_ActualThemeVariantChanged;
    }

    private void Current_ActualThemeVariantChanged(object? sender, EventArgs e)
    {
        if (Application.Current.ActualThemeVariant == ThemeVariant.Light)
        {
            _textMateInstallation.SetTheme(_registryOptions.LoadTheme(ThemeName.Light));
        }
        else if (Application.Current.ActualThemeVariant == ThemeVariant.Dark)
        {
            _textMateInstallation.SetTheme(_registryOptions.LoadTheme(ThemeName.DarkPlus));
        }
    }

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
                        new ToggleButton()
                            .IsChecked(@vm.WordWrap)
                            .ToolTip(Assets.Resources.PodLogsView_WordWrap)
                            .Content(new PathIcon() { Data = (Geometry)Application.Current.FindResource("text_wrap_regular") }),
                    ]),
                new TextEditor()
                    .Ref(out _textEditor)
                    .Row(1)
                    .Set(x => {
                        _textMateInstallation = _textEditor.InstallTextMate(_registryOptions, false);

                        x.Options.AllowScrollBelowDocument = false;
                        x.Options.ShowBoxForControlCharacters = false;
                        x.Options.EnableHyperlinks = false;
                        x.Options.EnableEmailHyperlinks = false;

                        x.TextChanged += (sender, e) => {
                            if (ViewModel?.AutoScrollToBottom == true)
                                _textEditor.ScrollToEnd();
                        };
                    })
                    .Document(@vm.Logs, BindingMode.OneWay)
                    .FontFamily(new FontFamily("Cascadia Mono"))
                    .FontSize(14.0)
                    .FontWeight(FontWeight.Normal)
                    .IsReadOnly(true)
                    .ShowLineNumbers(false)
                    .Background(new DynamicResourceExtension("SystemAltHighColor"))
                    .HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
                    .VerticalScrollBarVisibility(ScrollBarVisibility.Visible)
                    .WordWrap(@vm.WordWrap)
                    .ContextMenu(new ContextMenu()
                                    .Items([
                                        new MenuItem()
                                            .OnClick((_) => _textEditor.Copy())
                                            .Header(Assets.Resources.Action_Copy)
                                            .InputGesture(new KeyGesture(Key.C, KeyModifiers.Control))
                                            .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("copy_regular") }),
                                    ])
                    )
                ]);
    }

    public void SetOffset()
    {
        var sc = _textEditor.GetType().GetProperty("ScrollViewer", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_textEditor) as ScrollViewer;

        if (sc != null)
        {
            sc.Offset = ViewModel.ScrollOffset;
        }
    }

    public void GetOffset()
    {
        var sc = _textEditor.GetType().GetProperty("ScrollViewer", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_textEditor) as ScrollViewer;

        if (sc != null)
        {
            ViewModel.ScrollOffset = sc.Offset;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        SetOffset();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        GetOffset();
        base.OnUnloaded(e);

        Application.Current.ActualThemeVariantChanged -= Current_ActualThemeVariantChanged;
    }
}
