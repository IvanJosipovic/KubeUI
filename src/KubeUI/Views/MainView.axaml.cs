using System.Runtime.InteropServices;

namespace KubeUI.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        // Hide menu on macOS to mirror original IsVisible() predicate.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            MainMenu?.IsVisible = false;
        }
    }
}
