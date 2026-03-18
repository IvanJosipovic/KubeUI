using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Views;

public sealed partial class PodConsoleView2 : UserControl
{
    private readonly ILogger<PodConsoleView2> _logger;

    public PodConsoleViewModel2? ViewModel => DataContext as PodConsoleViewModel2;

    public PodConsoleView2()
    {
        _logger = Application.Current.GetRequiredService<ILogger<PodConsoleView2>>();

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


