using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Behaviors;

public sealed class PodConsoleConnectionBehavior : Behavior<Control>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject == null)
        {
            return;
        }

        AssociatedObject.AttachedToVisualTree += AssociatedObjectOnAttachedToVisualTree;
        AssociatedObject.DataContextChanged += AssociatedObjectOnDataContextChanged;
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.AttachedToVisualTree -= AssociatedObjectOnAttachedToVisualTree;
            AssociatedObject.DataContextChanged -= AssociatedObjectOnDataContextChanged;
        }

        base.OnDetaching();
    }

    private void AssociatedObjectOnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        ConnectAsync();
    }

    private void AssociatedObjectOnDataContextChanged(object? sender, EventArgs e)
    {
        if (AssociatedObject?.IsAttachedToVisualTree() == true)
        {
            ConnectAsync();
        }
    }

    private async void ConnectAsync()
    {
        if (AssociatedObject?.DataContext is not PodConsoleViewModel viewModel)
        {
            return;
        }

        try
        {
            await viewModel.ConnectAsync();
        }
        catch (Exception ex)
        {
            LogConnectionError(ex);
        }
    }

    private static void LogConnectionError(Exception ex)
    {
        if (Application.Current is not IServiceProviderHost host)
        {
            return;
        }

        var logger = host.Services.GetRequiredService<ILogger<PodConsoleConnectionBehavior>>();
        logger.LogError(ex, "Error connecting pod console.");
    }
}
