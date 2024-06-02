using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Core;

namespace KubeUI;

public sealed class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        var modelType = data.GetType();
        string? name;
        if (modelType.IsGenericType)
        {
            name = modelType.GetGenericTypeDefinition().FullName;
            name = name[..name.IndexOf('`')];
        }
        else
        {
            name = modelType.FullName;
        }

        name = name.Replace("ViewModel", "View");

        if (name is null)
        {
            return new TextBlock { Text = "Invalid Data Type" };
        }
        var type = Type.GetType(name);
        if (type is { })
        {
            var instance = Application.Current.GetRequiredService(type);
            if (instance is { })
            {
                return (Control)instance;
            }
            else
            {
                return new TextBlock { Text = "Create Instance Failed: " + type.FullName };
            }
        }
        else
        {
            return new TextBlock { Text = "Type Not Found: " + name };
        }
    }

    public bool Match(object? data)
    {
        return data is ObservableObject || data is IDockable;
    }
}
