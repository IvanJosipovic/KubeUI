using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Views;

public sealed partial class PodConsoleView : UserControl
{
    private readonly ILogger<PodConsoleView> _logger;

    public PodConsoleViewModel? ViewModel => DataContext as PodConsoleViewModel;

    public PodConsoleView()
    {
        _logger = Application.Current.GetRequiredService<ILogger<PodConsoleView>>();

        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (ViewModel != null)
            {
                await ViewModel.Connect();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting PodConsoleView2");
        }
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        // If ViewModel implements Dispose logic tied to lifecycle, caller (document close) should dispose it.
        // Nothing extra required here now.
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        // Auto-connect when DataContext swaps after initialization (optional safeguard)
        if (IsLoaded && ViewModel != null)
        {
            _ = ViewModel.Connect();
        }
    }
}


