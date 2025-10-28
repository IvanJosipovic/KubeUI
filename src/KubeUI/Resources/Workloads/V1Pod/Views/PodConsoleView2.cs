using Avalonia.Input;
using AvaloniaTerminal;
using KubeUI.Client;
using KubeUI.Views;

namespace KubeUI.Resources.Workloads.V1Pod.Views;

public sealed class PodConsoleView2 : MyViewBase<PodConsoleViewModel2>
{
    private readonly ILogger<PodConsoleView2> _logger;

    private readonly ISettingsService _settingsService;

    public PodConsoleView2(ILogger<PodConsoleView2> logger)
    {
        _logger = logger;
        _settingsService = Application.Current.GetRequiredService<ISettingsService>();
    }

    protected override object Build(PodConsoleViewModel2? vm)
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
                    ]),
                new TerminalControl()
                    .Set(x => {
                        x.Model = vm.Model;
                        _ = vm.Connect();
                    })
                    .Row(1)
                    .ContextMenu(new ContextMenu()
                                    .Items([
                                        //new MenuItem()
                                            //.OnClick((_) => _textEditor.Copy())
                                            //.Header(Assets.Resources.Action_Copy)
                                            //.Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("copy_regular") }),
                                        new MenuItem()
                                            .Command(vm.PasteCommand)
                                            .Header(Assets.Resources.Action_Paste)
                                            .InputGesture(new KeyGesture(Key.V, KeyModifiers.Control))
                                            .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("clipboard_paste_regular") }),
                                    ])
                    ),
            ]);
    }
}
