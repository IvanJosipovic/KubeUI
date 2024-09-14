using System.Diagnostics;
using Avalonia.Controls.Templates;
using Dock.Model.Core;
using KubeUI.Client;
using KubeUI.Views;

namespace KubeUI;

public sealed class ViewLocator : IDataTemplate
{
    private readonly ILogger<ViewLocator> _logger = Application.Current.GetRequiredService<ILogger<ViewLocator>>();
    private readonly Instrumentation _instrumentation = Application.Current.GetRequiredService<Instrumentation>();

    Type[] types;

    public Control Build(object? data)
    {
        var modelType = data.GetType();
        Type viewType;

        if (modelType.IsGenericType)
        {
            types ??= GetType().Assembly.GetExportedTypes();

            var genericModelType = modelType.GetGenericTypeDefinition();

            var name = genericModelType.FullName;
            name = name[..name.IndexOf('`')];
            name = name.Replace("ViewModel", "View");

            viewType = Type.GetType(name);

            if (viewType == null)
            {
                var genericViewType = types.FirstOrDefault(x =>
                    x.BaseType?.Name.Equals(typeof(MyViewBase<object>).Name) == true &&
                    (
                        (x.BaseType.GenericTypeArguments[0].IsGenericType && x.BaseType?.GenericTypeArguments[0].GetGenericTypeDefinition() == genericModelType)
                        ||
                        (x.BaseType?.GenericTypeArguments[0] == genericModelType)
                    )
                );

                if (genericViewType != null)
                {
                    viewType = genericViewType.MakeGenericType(modelType.GenericTypeArguments[0]);
                }
            }
        }
        else
        {
            viewType = Type.GetType(modelType.FullName.Replace("ViewModel", "View"));
        }

        if (viewType is { })
        {
            var instance = Application.Current.GetRequiredService(viewType);
            if (instance is { })
            {
                _instrumentation.ViewOpened.Add(1, new TagList()
                {
                    { "view", viewType.Name }
                });
                return (Control)instance;
            }
        }

        _logger.LogCritical("Unable to load View for ViewModel: {view}", modelType.FullName);

        return new TextBlock { Text = "Unable to load View for ViewModel: " + modelType.FullName };
    }

    public bool Match(object? data)
    {
        return data is ObservableObject || data is IDockable;
    }
}
