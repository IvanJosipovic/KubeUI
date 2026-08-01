namespace KubeUI.Testing.Kubernetes.Transport;

internal sealed class TestConditionHandler : DelegatingHandler
{
    private readonly TimeSpan _responseLatency;
    private readonly bool _throwOnConnect;

    public TestConditionHandler(TimeSpan responseLatency, bool throwOnConnect)
    {
        _responseLatency = responseLatency;
        _throwOnConnect = throwOnConnect;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (_responseLatency <= TimeSpan.Zero && !_throwOnConnect)
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
