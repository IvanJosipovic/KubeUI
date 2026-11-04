using KubeUI.Features.Clusters.Workspace;
using Shouldly;

namespace KubeUI.Features.Clusters.Tests.Workspace;

public sealed class PermissionRefreshCoordinatorTests
{
    [Fact]
    public async Task queued_requests_coalesce_into_one_full_refresh_pass()
    {
        List<PermissionRefreshScope> passes = [];
        var firstPass = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var coordinator = CreateCoordinator(
            (scope, _) =>
            {
                passes.Add(scope);
                firstPass.TrySetResult();
                return Task.CompletedTask;
            });

        coordinator.Request(PermissionRefreshScope.Namespaces);
        coordinator.Request(PermissionRefreshScope.All);

        await firstPass.Task.WaitAsync(TestContext.Current.CancellationToken);
        await TestWait.UntilAsync(
            () => passes.Count == 1,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        passes.ShouldBe([PermissionRefreshScope.All]);
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
        var coordinator = CreateCoordinator(
            (_, _) =>
            {
                passes++;
                return Task.CompletedTask;
            },
            () => canRefresh);

        coordinator.Dispose();
        coordinator.Request(PermissionRefreshScope.All);

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
        Func<bool>? canRefresh = null)
    {
        return new PermissionRefreshCoordinator(
            refreshAsync,
            canRefresh ?? (static () => true),
            static _ => Task.CompletedTask,
            CancellationToken.None);
    }
}
