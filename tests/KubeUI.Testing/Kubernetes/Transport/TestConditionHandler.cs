namespace KubeUI.Testing.Kubernetes.Transport;

internal sealed class TestConditionHandler : DelegatingHandler
{
    private readonly TimeSpan _responseLatency;
    private readonly bool _throwOnConnect;
    private int _enabled;

    public TestConditionHandler(TimeSpan responseLatency, bool throwOnConnect)
    {
        _responseLatency = responseLatency;
        _throwOnConnect = throwOnConnect;
    }

    public void Enable() => Volatile.Write(ref _enabled, 1);

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (Volatile.Read(ref _enabled) == 0)
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        if (_responseLatency > TimeSpan.Zero)
        {
            using PeriodicTimer timer = new(_responseLatency);
            await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false);
        }

        if (_throwOnConnect)
        {
            throw new HttpRequestException("simulated connection failure");
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
