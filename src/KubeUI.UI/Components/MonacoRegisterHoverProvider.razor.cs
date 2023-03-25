namespace KubeUI.UI.Components
{
    public partial class MonacoRegisterHoverProvider
    {
        [Inject]
        IJSRuntime jsRuntime { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", $"./_content/KubeUI.UI/Components/{GetType().Name}.razor.js");
                await module.InvokeVoidAsync("registerHoverProvider", "yaml", DotNetObjectReference.Create(this));
            }
        }

        [JSInvokable]
        public async Task<object> ProvideHoverAsync(string model, Position position)
        {
            return new {
                contents = new[]
                {
                    new
                    {
                        value = $"This is a tooltip at {position.lineNumber} - {position.column}"
                    }
                }
            };
        }
    }

    public class Position
    {
        public int lineNumber { get; set; }
        public int column { get; set; }
    }
}