namespace KubeUI.Testing.Utilities;

public static class TestWait
{
    public static Task UntilAsync(
        Func<bool> predicate,
        int timeoutMs,
        CancellationToken cancellationToken = default,
        Action? beforePoll = null)
    {
        return UntilAsync(
            predicate,
            TimeSpan.FromMilliseconds(timeoutMs),
            cancellationToken: cancellationToken,
            beforePoll: beforePoll);
    }

    public static async Task NextPollAsync(TimeSpan pollInterval, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pollInterval, TimeSpan.Zero);
        using var timer = new PeriodicTimer(pollInterval);
        await timer.WaitForNextTickAsync(cancellationToken);
    }

    public static async Task UntilAsync(
        Func<bool> predicate,
        TimeSpan timeout,
        TimeSpan? pollInterval = null,
        CancellationToken cancellationToken = default,
        Action? beforePoll = null)
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
                beforePoll?.Invoke();
                if (predicate())
                {
                    return;
                }

                var remaining = deadline - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero || !await interval.WaitForNextTickAsync(cancellationToken))
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

    public static async Task<T?> UntilValueAsync<T>(
        Func<T?> valueFactory,
        TimeSpan timeout,
        TimeSpan? pollInterval = null,
        CancellationToken cancellationToken = default,
        Action? beforePoll = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(valueFactory);
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
                beforePoll?.Invoke();
                var value = valueFactory();
                if (value is not null)
                {
                    return value;
                }

                var remaining = deadline - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero || !await interval.WaitForNextTickAsync(cancellationToken))
                {
                    break;
                }
            }
        }
        finally
        {
            interval.Dispose();
        }

        beforePoll?.Invoke();
        return valueFactory();
    }

    public static Task<T?> UntilValueAsync<T>(
        Func<T?> valueFactory,
        int timeoutMs,
        CancellationToken cancellationToken = default,
        Action? beforePoll = null)
        where T : class
    {
        return UntilValueAsync(
            valueFactory,
            TimeSpan.FromMilliseconds(timeoutMs),
            cancellationToken: cancellationToken,
            beforePoll: beforePoll);
    }
}
