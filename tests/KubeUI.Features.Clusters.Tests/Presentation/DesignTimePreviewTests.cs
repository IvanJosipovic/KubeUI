using Avalonia.Controls;
using Avalonia.Headless.XUnit;

namespace KubeUI.Features.Clusters.Tests.Presentation;

public sealed class DesignTimePreviewTests
{
    [AvaloniaFact]
    public void IsDesignMode_matches_Avalonia_design_mode()
    {
        Assert.Equal(Design.IsDesignMode, DesignTimePreview.IsDesignMode);
    }
}
