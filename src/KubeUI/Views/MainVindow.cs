namespace KubeUI.Views;

public static class MainWindow
{
    public static Window Build() =>
        (Window)new Window()
            .Title("KubeUI")
            .TransparencyLevelHint(new List<WindowTransparencyLevel>() { WindowTransparencyLevel.AcrylicBlur })
            .UseLayoutRounding(true)
            .Content(
                new Panel()
                    .Children([
                        new ExperimentalAcrylicBorder()
                            .IsHitTestVisible(false)
                            .Material(
                                new ExperimentalAcrylicMaterial()
                                    .BackgroundSource(AcrylicBackgroundSource.Digger)
                                    .FallbackColor("AcrylicFallbackColor".GetDynamicResource())
                                    .MaterialOpacity(0.80)
                                    .TintColor("SystemAltHighColor".GetDynamicResource())
                                    .TintOpacity(0.0)
                                ),
                        new MainView()
                    ]
                )
            );
}
