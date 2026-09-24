using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls.Templates;
using Dock.Model.Core;
using KubeUI.Avalonia.Features.AI;
using KubeUI.Avalonia.Features.Clusters.Catalog;
using KubeUI.Avalonia.Features.Clusters.Error;
using KubeUI.Avalonia.Features.Clusters.Overview;
using KubeUI.Avalonia.Features.Clusters.Settings;
using KubeUI.Avalonia.Features.Resources.List;
using KubeUI.Avalonia.Features.Resources.Properties;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Views;
using KubeUI.Avalonia.Shell.Documents.About;
using KubeUI.Avalonia.Shell.Documents.CloudClusters.Aks;
using KubeUI.Avalonia.Shell.Documents.Settings;
using KubeUI.Avalonia.Shell.Main;
using KubeUI.Avalonia.Shell.Navigation;

namespace KubeUI.Avalonia.Infrastructure.Presentation;

sealed class ViewLocator : IDataTemplate
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ViewLocator> _logger;
    private readonly Instrumentation _instrumentation;

    public ViewLocator(IServiceProvider serviceProvider, ILogger<ViewLocator> logger, Instrumentation instrumentation)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _instrumentation = instrumentation;
    }

    public Control Build(object? data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var modelType = data.GetType();
        var instance = ResolveView(data);

        if (instance is not null)
        {
            _instrumentation.ViewOpened.Add(1, new TagList { { "view", GetViewMetricName(instance.GetType(), modelType) } });
            return instance;
        }

        _logger.LogCritical("Unable to load View for ViewModel: {ViewModel}", modelType.FullName);
        return new TextBlock { Text = "Unable to load View for ViewModel: " + modelType.FullName };
    }

    public bool Match(object? data)
    {
        return data is ObservableObject or IDockable;
    }

    private Control? ResolveView(object model)
    {
        if (model is IViewModelViewFactory viewFactory)
        {
            return viewFactory.CreateView(_serviceProvider);
        }

        return model switch
        {
            IResourceListViewModel => _serviceProvider.GetRequiredService<ResourceListView>(),
            MainViewModel => _serviceProvider.GetRequiredService<MainView>(),
            HomeViewModel => _serviceProvider.GetRequiredService<HomeView>(),
            NavigationViewModel => _serviceProvider.GetRequiredService<NavigationView>(),
            ClusterListViewModel => _serviceProvider.GetRequiredService<ClusterListView>(),
            ClusterErrorViewModel => _serviceProvider.GetRequiredService<ClusterErrorView>(),
            ClusterViewModel => _serviceProvider.GetRequiredService<ClusterView>(),
            ClusterSettingsViewModel => _serviceProvider.GetRequiredService<ClusterSettingsView>(),
            VisualizationViewModel => _serviceProvider.GetRequiredService<VisualizationView>(),
            ResourceYamlViewModel => _serviceProvider.GetRequiredService<ResourceYamlView>(),
            AgentChatViewModel => _serviceProvider.GetRequiredService<AgentChatView>(),
            SettingsViewModel => _serviceProvider.GetRequiredService<SettingsView>(),
            AboutViewModel => _serviceProvider.GetRequiredService<AboutView>(),
            ImportAksClusterViewModel => _serviceProvider.GetRequiredService<ImportAksClusterView>(),
            PodConsoleViewModel => _serviceProvider.GetRequiredService<PodConsoleView>(),
            PortForwarderListViewModel => _serviceProvider.GetRequiredService<PortForwarderListView>(),
            PodLogsViewModel => _serviceProvider.GetRequiredService<PodLogsView>(),
            _ => null
        };
    }

    private static string GetPrettyName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var genericTypeName = type.GetGenericTypeDefinition().Name;
        var genericTypeIndex = genericTypeName.IndexOf('`');
        if (genericTypeIndex >= 0)
        {
            genericTypeName = genericTypeName[..genericTypeIndex];
        }

        var argumentNames = type.GetGenericArguments().Select(GetPrettyName);
        return $"{genericTypeName}<{string.Join(", ", argumentNames)}>";
    }

    private static string GetViewMetricName(Type viewType, Type modelType)
    {
        if (viewType.IsGenericType || !modelType.IsGenericType)
        {
            return GetPrettyName(viewType);
        }

        var argumentNames = modelType.GetGenericArguments().Select(GetPrettyName);
        return $"{viewType.Name}<{string.Join(", ", argumentNames)}>";
    }

}

internal interface IViewModelViewFactory
{
    Control CreateView(IServiceProvider serviceProvider);
}



