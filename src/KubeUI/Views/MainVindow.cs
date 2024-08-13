namespace KubeUI.Views;

public static class MainWindow
{
    public static Window Build() =>
        (Window)new Window()
            .Title("KubeUI")
            .UseLayoutRounding(true)
            .Content(
                new Panel()
                    .Children([
                        new MainView()
                    ]
                )
            );
}
