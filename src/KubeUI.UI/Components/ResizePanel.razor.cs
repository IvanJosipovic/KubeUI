using Microsoft.JSInterop;
using MudBlazor.Services;

namespace KubeUI.UI.Components
{
    public partial class ResizePanel : IDisposable
    {
        [Inject]
        private IResizeListenerService ResizeListenerService { get; set; }

        [Inject]
        private IJSRuntime JS { get; set; }

        [Parameter]
        public RenderFragment<string> ChildContent { get; set; }

        [Parameter]
        public int Offset { get; set; }

        [Parameter]
        public bool SetHeigh { get; set; }

        private IJSObjectReference? module;

        private ElementReference ReferenceToDiv;

        private string? Height { get; set; } = "1px";

        protected override void OnInitialized()
        {
            ResizeListenerService.OnResized += ResizeListenerService_OnResized;
        }

        private async void ResizeListenerService_OnResized(object? sender, BrowserWindowSize e)
        {
            Update(e.Height);
        }

        private async void Update(int height)
        {
            var elementY = await module.InvokeAsync<decimal>("getElementY", ReferenceToDiv);
            Height = $"{height - elementY - Offset}px";
            await InvokeAsync(StateHasChanged);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                module = await JS.InvokeAsync<IJSObjectReference>("import", $"./_content/KubeUI.UI/Components/{GetType().Name}.razor.js");
                var bs = await ResizeListenerService.GetBrowserWindowSize();
                Update(bs.Height);
            }
        }

        public void Dispose()
        {
            ResizeListenerService.OnResized -= ResizeListenerService_OnResized;
        }
    }
}