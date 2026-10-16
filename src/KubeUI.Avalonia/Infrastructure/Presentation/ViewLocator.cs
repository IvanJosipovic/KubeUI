using System.Diagnostics;
using Avalonia.Controls.Templates;
using Dock.Model.Core;
using KubeUI.Avalonia.Infrastructure.Platform;

namespace KubeUI.Avalonia.Infrastructure.Presentation;

sealed class ViewLocator : IDataTemplate
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ViewLocator> _logger;
    private readonly Instrumentation _instrumentation;
    private readonly IViewFactory[] _viewFactories;

    public ViewLocator(
        IServiceProvider serviceProvider,
        ILogger<ViewLocator> logger,
        Instrumentation instrumentation,
        IViewFactory[]? viewFactories = null)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _instrumentation = instrumentation;
        _viewFactories = viewFactories ?? [];
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

        foreach (var factory in _viewFactories)
        {
            if (factory.CanCreate(model))
            {
                return factory.Create(_serviceProvider);
            }
        }

        return null;
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

/// <summary>Creates a view for a view model using the application's service provider.</summary>
public interface IViewModelViewFactory
{
    /// <summary>Creates the view associated with this view model.</summary>
    /// <param name="serviceProvider">Application services used to construct the view.</param>
    /// <returns>Created view.</returns>
    Control CreateView(IServiceProvider serviceProvider);
}
