using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace KubeUI.Services
{
    public class AppInsights : IAppInsights
    {
        private ILogger<AppInsights> Logger;
        private NavigationManager NavigationManager;
        private IJSRuntime JSRuntime;

        public AppInsights(ILogger<AppInsights> logger, IJSRuntime jsRuntime, NavigationManager navigationManager)
        {
            Logger = logger;
            JSRuntime = jsRuntime;
            NavigationManager = navigationManager;
            NavigationManager.LocationChanged += NavigationManager_LocationChanged;
        }

        private void NavigationManager_LocationChanged(object sender, LocationChangedEventArgs e)
        {
            JSRuntime.InvokeAsync<object>("trackPageView");
        }

        public void TrackEvent(string name)
        {
            JSRuntime.InvokeAsync<object>("trackEvent", name);
        }
    }
}
