using KubeUI.Core.Client;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace KubeUI.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .RegisterBlazorMauiWebView()
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        ConfigureServices.Configure(builder.Configuration, builder.Services);

        builder.Services.AddBlazorWebView();

        return builder.Build();
    }
}
