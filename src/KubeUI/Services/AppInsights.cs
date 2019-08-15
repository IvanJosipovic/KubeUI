using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace KubeUI.Services
{
    public class AppInsights : IAppInsights
    {
        private ILogger<AppInsights> Logger;
        private IUriHelper UriHelper;
        private IJSRuntime JSRuntime;

        public AppInsights(ILogger<AppInsights> logger, IJSRuntime jsRuntime, IUriHelper uriHelper)
        {
            Logger = logger;
            JSRuntime = jsRuntime;
            UriHelper = uriHelper;
            UriHelper.OnLocationChanged += UriHelper_OnLocationChanged;
        }

        private void UriHelper_OnLocationChanged(object sender, LocationChangedEventArgs e)
        {
            JSRuntime.InvokeAsync<object>("trackPageView");
        }

        public void TrackEvent(string name)
        {
            JSRuntime.InvokeAsync<object>("trackEvent", name);
        }
    }
}
