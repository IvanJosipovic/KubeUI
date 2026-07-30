namespace KubeUI.Testing;

public static class TestWait
{
    public static async Task NextPollAsync(TimeSpan pollInterval, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pollInterval, TimeSpan.Zero);
        using var timer = new PeriodicTimer(pollInterval);
        await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async Task UntilAsync(
        Func<bool> predicate,
        TimeSpan timeout,
        TimeSpan? pollInterval = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout));
        }

        var deadline = DateTime.UtcNow + timeout;
        var interval = new PeriodicTimer(pollInterval ?? TimeSpan.FromMilliseconds(25));
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (predicate())
                {
                    return;
                }

                var remaining = deadline - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero || !await interval.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
                {
                    break;
                }
            }
        }
        finally
        {
            interval.Dispose();
        }

        if (!predicate())
        {
            throw new TimeoutException($"The test condition was not met within {timeout}.");
        }
    }
}
