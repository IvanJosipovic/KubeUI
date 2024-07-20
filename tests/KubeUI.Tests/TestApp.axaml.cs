using Avalonia;
using Avalonia.Markup.Xaml;
using KubeUI.Client;
using KubeUI.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Dock.Model.Core;
using KubernetesCRDModelGen;

namespace KubeUI.Tests;

public class TestApp : Application
{
    public override void Initialize()
    {
        var builder = new ConfigurationBuilder();
        var root = builder.Build();

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(root);

        services.AddLogging();

        // Services
        services.AddSingleton<ClusterManager>();
        services.AddTransient<Cluster>();
        services.AddTransient<ModelCache>();
        services.AddSingleton<IGenerator, Generator>();
        services.AddSingleton<IFactory, DockFactory>();

        // Windows
        services.AddTransient<MainWindow>();

        var provider = services.BuildServiceProvider();
        Resources[typeof(IServiceProvider)] = provider;

        AvaloniaXamlLoader.Load(this);
    }
}
