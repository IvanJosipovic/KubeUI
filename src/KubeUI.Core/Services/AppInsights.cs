using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Threading.Tasks;

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

        private async void NavigationManager_LocationChanged(object sender, LocationChangedEventArgs e)
        {
            await JSRuntime.InvokeVoidAsync("appInsights.trackPageView");
        }

        public async Task TrackEvent(string name)
        {
            await JSRuntime.InvokeVoidAsync("appInsights.trackEvent", new { name = name });
        }
    }
}
