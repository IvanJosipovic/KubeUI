using Avalonia.LogicalTree;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;

namespace KubeUI.Avalonia.Features.Resources.Properties;

internal static class ResourcePropertiesViewRefresher
{
    internal static IEnumerable<Control> EnumerateControls(Control root)
    {
        var stack = new Stack<Control>();
        var seen = new HashSet<Control>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!seen.Add(current))
            {
                continue;
            }

            yield return current;

            switch (current)
            {
                case Panel panel:
                    foreach (var child in panel.Children.OfType<Control>())
                    {
                        stack.Push(child);
                    }
                    break;
                case Decorator decorator when decorator.Child is Control child:
                    stack.Push(child);
                    break;
                case ContentControl contentControl when contentControl.Content is Control child:
                    stack.Push(child);
                    break;
            }

            if (current is not ILogical logical)
            {
                continue;
            }

            foreach (var child in logical.LogicalChildren.OfType<Control>())
            {
                stack.Push(child);
            }
        }
    }

    public static void Refresh<TResource>(Control root, TResource resource)
        where TResource : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(resource);

        foreach (var metricsControl in EnumerateControls(root).OfType<MetricsControl>())
        {
            metricsControl.UpdateResourceSnapshot(resource);
        }
    }
}
