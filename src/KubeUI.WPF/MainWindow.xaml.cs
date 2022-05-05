using KubeUI.Core.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Windows;

namespace KubeUI.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private IHost host;

    public MainWindow()
    {
        host = Host.CreateDefaultBuilder()
        .ConfigureServices((hostBuilder, services) =>
        {
            services.AddWpfBlazorWebView();

#if DEBUG
            services.AddBlazorWebViewDeveloperTools();
#endif
            ConfigureServices.Configure(hostBuilder.Configuration, services);

            services.AddLogging(config => config.AddFile("Logs/{Date}.txt"));
        })
        .Build();

        host.RunAsync();

        Resources.Add("services", host.Services);

        InitializeComponent();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        host.StopAsync().Wait();
    }
}
