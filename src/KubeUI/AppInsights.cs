using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace KubeUI
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
            UriHelper.OnLocationChanged += OnLocationChanges;
        }

        private void OnLocationChanges(object sender, string location)
        {
            JSRuntime.InvokeAsync<object>("trackPageView");
        }

        public void TrackEvent(string name)
        {
            JSRuntime.InvokeAsync<object>("trackEvent", name);
        }
    }
}
