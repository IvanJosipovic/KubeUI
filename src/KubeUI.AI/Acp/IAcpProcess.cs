namespace KubeUI.AI.Acp;

internal interface IAcpProcess : IAsyncDisposable
{
    Stream Input { get; }
    Stream Output { get; }

    event Action<string>? ErrorReceived;
    event Action? Exited;
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
