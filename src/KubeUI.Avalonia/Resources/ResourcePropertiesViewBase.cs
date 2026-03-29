using Avalonia.Controls;
using KubeUI.Avalonia.Controls;
using k8s;
using k8s.Models;

namespace KubeUI.Avalonia.Resources;

public abstract partial class ResourcePropertiesViewBase<T> : UserControl
    where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        Reload();
    }

    protected void Reload()
    {
        StackPanel? items = this.FindControl<StackPanel>("PART_Items");
        if (items == null)
        {
            return;
        }

        items.Children.Clear();

        if (DataContext is not T resource)
        {
            return;
        }

        Build(resource);
    }

    protected abstract void Build(T resource);

    protected void Append(params Control[] controls)
    {
        StackPanel? items = this.FindControl<StackPanel>("PART_Items");
        if (items == null)
        {
            return;
        }

        foreach (var control in controls)
        {
            items.Children.Add(control);
        }
    }

    protected static PropertyItem Property(string key, object? value)
    {
        return new PropertyItem
        {
            Key = key,
            Value = value?.ToString() ?? string.Empty
        };
    }

    protected static HeaderItem Header(string text)
    {
        return new HeaderItem
        {
            Text = text
        };
    }

    protected static string JoinStrings(IEnumerable<string>? values)
    {
        if (values == null)
        {
            return string.Empty;
        }

        return string.Join(", ", values.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    protected static string JoinPairs(IEnumerable<KeyValuePair<string, string>>? values)
    {
        if (values == null)
        {
            return string.Empty;
        }

        return string.Join(", ", values.Select(pair => $"{pair.Key}={pair.Value}"));
    }
}
