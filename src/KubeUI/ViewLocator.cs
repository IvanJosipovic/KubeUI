using Avalonia.Controls.Templates;
using Dock.Model.Core;
using KubeUI.Views;

namespace KubeUI;

public sealed class ViewLocator : IDataTemplate
{
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
                return (Control)instance;
            }

            return new TextBlock { Text = "Create Instance Failed: " + viewType.FullName };
        }

        return new TextBlock { Text = "View not found for: " + modelType.FullName };
    }

    public bool Match(object? data)
    {
        return data is ObservableObject || data is IDockable;
    }
}
