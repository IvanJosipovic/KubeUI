using KubeUI.Features.Clusters.Workspace;
using Shouldly;

namespace KubeUI.Features.Clusters.Tests.Workspace;

public sealed class PermissionRefreshCoordinatorTests
{
    [Fact]
    public async Task requests_queued_while_the_worker_is_debouncing_coalesce_into_one_full_pass()
    {
        List<PermissionRefreshScope> passes = [];
        var workerConsumedFirstRequest = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseDebounce = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var coordinator = CreateCoordinator(
            (scope, _) =>
            {
                passes.Add(scope);
                return Task.CompletedTask;
            },
            debounce: _ =>
            {
                workerConsumedFirstRequest.TrySetResult();
                return releaseDebounce.Task;
            });

        coordinator.Request(PermissionRefreshScope.Namespaces);
        await workerConsumedFirstRequest.Task.WaitAsync(TestContext.Current.CancellationToken);

        coordinator.Request(PermissionRefreshScope.All);
        coordinator.Request(PermissionRefreshScope.Namespaces);

        releaseDebounce.TrySetResult();

        await TestWait.UntilAsync(
            () => passes.Count == 2,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        passes.ShouldBe([PermissionRefreshScope.Namespaces, PermissionRefreshScope.All]);
    }

    [Fact]
    public async Task request_during_active_refresh_schedules_one_follow_up_pass()
    {
        var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        List<PermissionRefreshScope> passes = [];
        var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondCompleted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var coordinator = CreateCoordinator(
            async (scope, _) =>
            {
                passes.Add(scope);
                if (passes.Count == 1)
                {
                    firstStarted.TrySetResult();
                    await releaseFirst.Task.WaitAsync(TestContext.Current.CancellationToken);
                    return;
                }

                secondCompleted.TrySetResult();
            });

        coordinator.Request(PermissionRefreshScope.Namespaces);
        await firstStarted.Task.WaitAsync(TestContext.Current.CancellationToken);

        coordinator.Request(PermissionRefreshScope.Namespaces);
        coordinator.Request(PermissionRefreshScope.All);
        releaseFirst.TrySetResult();

        await secondCompleted.Task.WaitAsync(TestContext.Current.CancellationToken);

        passes.ShouldBe([PermissionRefreshScope.Namespaces, PermissionRefreshScope.All]);
    }

    [Fact]
    public async Task invalidation_is_ignored_while_a_full_refresh_is_running()
    {
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var passes = 0;
        using var coordinator = CreateCoordinator(
            async (_, _) =>
            {
                passes++;
                started.TrySetResult();
                await release.Task.WaitAsync(TestContext.Current.CancellationToken);
            });

        var refresh = coordinator.RefreshAsync(TestContext.Current.CancellationToken);
        await started.Task.WaitAsync(TestContext.Current.CancellationToken);

        coordinator.IsRefreshInProgress.ShouldBeTrue();
        coordinator.Invalidate();

        release.TrySetResult();
        await refresh.WaitAsync(TestContext.Current.CancellationToken);
        coordinator.IsRefreshInProgress.ShouldBeFalse();
        passes.ShouldBe(1);
    }

    [Fact]
    public async Task disposed_coordinator_stops_scheduling_passes()
    {
        var canRefresh = true;
        var passes = 0;
        using var coordinator = CreateCoordinator(
            (_, _) =>
            {
                passes++;
                return Task.CompletedTask;
            },
            () => canRefresh);

        coordinator.Dispose();
        coordinator.Request(PermissionRefreshScope.All);

        // Give a debounced (incorrect) pass a bounded window to fire before the negative assert.
        await TestWait.NextPollAsync(TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken);
        passes.ShouldBe(0);
        coordinator.IsRefreshInProgress.ShouldBeFalse();
    }

    [Fact]
    public async Task request_is_ignored_when_owner_cannot_refresh()
    {
        var passes = 0;
        using var coordinator = CreateCoordinator(
            (_, _) =>
            {
                passes++;
                return Task.CompletedTask;
            },
            () => false);

        coordinator.Request(PermissionRefreshScope.All);
        await coordinator.RefreshAsync(TestContext.Current.CancellationToken);

        passes.ShouldBe(0);
    }

    private static PermissionRefreshCoordinator CreateCoordinator(
        Func<PermissionRefreshScope, CancellationToken, Task> refreshAsync,
        Func<bool>? canRefresh = null,
        Func<CancellationToken, Task>? debounce = null)
    {
        return new PermissionRefreshCoordinator(
            refreshAsync,
            canRefresh ?? (static () => true),
            debounce ?? (static _ => Task.CompletedTask),
            CancellationToken.None);
    }
}
