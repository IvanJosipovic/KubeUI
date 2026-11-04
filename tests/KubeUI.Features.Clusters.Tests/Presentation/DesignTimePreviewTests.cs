using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Shouldly;

namespace KubeUI.Features.Clusters.Tests.Presentation;

public sealed class DesignTimePreviewTests
{
    [AvaloniaFact]
    public void IsDesignMode_matches_Avalonia_design_mode()
    {
        DesignTimePreview.IsDesignMode.ShouldBe(Design.IsDesignMode);
    }
}
