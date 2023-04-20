namespace KubeUI.UI.Components;

public partial class Age<TItem> : IDisposable where TItem : class, IKubernetesObject<V1ObjectMeta>
{
    [Parameter]
    public DateTime? Date { get; set; }

    private TimeSpan Delta => DateTime.UtcNow - Date.Value;

    private System.Timers.Timer Timer { get; set; }

    protected override void OnInitialized()
    {
        Timer = new System.Timers.Timer(TimeSpan.FromSeconds(1));
        Timer.Elapsed += Timer_Elapsed;
        Timer.Enabled = true;
        Timer.AutoReset = true;
    }

    private async void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        Timer?.Dispose();
    }
}