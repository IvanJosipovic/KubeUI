using Avalonia;
using Avalonia.Markup.Xaml;
using KubeUI.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Dock.Model.Core;
using KubernetesCRDModelGen;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using KubeUI.ViewModels;
using KubeUI.Views;

namespace KubeUI.Tests;

public class TestApp : Application
{
    public override void Initialize()
    {
        var builder = new ConfigurationBuilder();
        var root = builder.Build();

        var services = new ServiceCollection();

        services.AddLogging();

        // Services
        services.AddSingleton<ClusterManager>();
        services.AddTransient<ICluster, Cluster>();
        services.AddTransient<ModelCache>();
        services.AddSingleton<IGenerator, Generator>();
        services.AddSingleton<IFactory, DockFactory>();

        services.Scan(scan => scan
            .FromAssemblyOf<App>()
            .AddClasses(classes => classes.AssignableToAny([typeof(UserControl), typeof(ObservableObject), typeof(ViewModelBase), typeof(MyViewBase<>)]))
            .AsSelf()
            .WithTransientLifetime()
        );

        var provider = services.BuildServiceProvider();
        Resources[typeof(IServiceProvider)] = provider;

        AvaloniaXamlLoader.Load(this);
    }
}
