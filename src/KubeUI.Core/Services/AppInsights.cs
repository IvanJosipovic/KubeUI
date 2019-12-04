using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace KubeUI.Services
{
    public class AppInsights : IAppInsights
    {
        private NavigationManager NavigationManager;
        private IJSRuntime JSRuntime;

        public AppInsights(IJSRuntime jsRuntime, NavigationManager navigationManager)
        {
            JSRuntime = jsRuntime;
            NavigationManager = navigationManager;
            NavigationManager.LocationChanged += NavigationManager_LocationChanged;
        }

        private void NavigationManager_LocationChanged(object sender, LocationChangedEventArgs e)
        {
            JSRuntime.InvokeVoidAsync("trackPageView");
        }

        public void TrackEvent(string name)
        {
            JSRuntime.InvokeVoidAsync("trackEvent", name);
        }
    }
}
