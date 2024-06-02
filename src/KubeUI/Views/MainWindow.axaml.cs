using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace KubeUI.Views;

[ServiceDescriptor<MainWindow>(ServiceLifetime.Transient)]
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
