using Avalonia.Controls;
using Avalonia.Headless.XUnit;

namespace KubeUI.Avalonia.Tests.Features.Clusters.Presentation;

public sealed class DesignTimePreviewTests
{
    [AvaloniaFact]
    public void IsDesignMode_matches_Avalonia_design_mode()
    {
        Assert.Equal(Design.IsDesignMode, DesignTimePreview.IsDesignMode);
    }
}
