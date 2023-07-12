
namespace KubeUI.UI.Components
{
    public partial class ResizePanel : IAsyncDisposable
    {
        [Inject]
        private IBrowserViewportService BrowserViewportService { get; set; }

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

        private Guid _subscriptionId = Guid.NewGuid();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                module = await JS.InvokeAsync<IJSObjectReference>("import", $"./_content/KubeUI.UI/Components/{GetType().Name}.razor.js");
                await BrowserViewportService.SubscribeAsync(_subscriptionId, Resize, new ResizeOptions() { NotifyOnBreakpointOnly = false });
            }
        }

        private async Task Resize(BrowserViewportEventArgs args)
        {
            var elementY = await module.InvokeAsync<decimal>("getElementY", ReferenceToDiv);
            Height = $"{args.BrowserWindowSize.Height - elementY - Offset}px";
            await InvokeAsync(StateHasChanged);
        }

        public async ValueTask DisposeAsync()
        {
            await BrowserViewportService.UnsubscribeAsync(_subscriptionId);
        }
    }
}
