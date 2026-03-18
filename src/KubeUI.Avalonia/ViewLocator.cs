using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls.Templates;
using Dock.Model.Core;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia;

public sealed class ViewLocator : IDataTemplate
{
    private readonly ILogger<ViewLocator> _logger;
    private readonly Instrumentation _instrumentation;
    private Type[]? _types;

    public ViewLocator(ILogger<ViewLocator> logger, Instrumentation instrumentation)
    {
        _logger = logger;
        _instrumentation = instrumentation;
    }

    public Control Build(object? data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var modelType = data.GetType();
        var viewType = ResolveViewType(modelType);

        if (viewType is not null)
        {
            var instance = Application.Current.GetRequiredService(viewType);
            _instrumentation.ViewOpened.Add(1, new TagList { { "view", GetPrettyName(instance.GetType()) } });
            return (Control)instance;
        }

        _logger.LogCritical("Unable to load View for ViewModel: {ViewModel}", modelType.FullName);
        return new TextBlock { Text = "Unable to load View for ViewModel: " + modelType.FullName };
    }

    public bool Match(object? data)
    {
        return data is ObservableObject or IDockable;
    }

    private Type? ResolveViewType(Type modelType)
    {
        var expectedName = GetUnboundFullName(modelType).Replace("ViewModel", "View", StringComparison.Ordinal);
        var viewType = GetViewTypes().FirstOrDefault(type => string.Equals(GetUnboundFullName(type), expectedName, StringComparison.Ordinal));

        if (viewType is null)
        {
            return null;
        }

        if (viewType.IsGenericTypeDefinition && modelType.IsGenericType)
        {
            return viewType.MakeGenericType(modelType.GetGenericArguments());
        }

        return viewType;
    }

    private Type[] GetViewTypes()
    {
        if (_types is not null)
        {
            return _types;
        }

        _types = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => assembly.GetName().Name?.StartsWith("KubeUI", StringComparison.Ordinal) == true)
            .SelectMany(GetExportedTypes)
            .Where(type => type is not null)
            .Cast<Type>()
            .ToArray();

        return _types;
    }

    private static IEnumerable<Type?> GetExportedTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetExportedTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types;
        }
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

    private static string GetUnboundFullName(Type type)
    {
        var fullName = type.IsGenericType ? type.GetGenericTypeDefinition().FullName : type.FullName;
        ArgumentNullException.ThrowIfNull(fullName);

        var genericTypeIndex = fullName.IndexOf('`');
        return genericTypeIndex >= 0 ? fullName[..genericTypeIndex] : fullName;
    }
}





