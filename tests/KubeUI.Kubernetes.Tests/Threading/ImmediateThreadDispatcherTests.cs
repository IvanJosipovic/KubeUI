using System.Reactive.Concurrency;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Threading;

public sealed class ImmediateThreadDispatcherTests
{
    [Fact]
    public void Invoke_executes_action_synchronously()
    {
        var dispatcher = new ImmediateThreadDispatcher();
        var executed = false;

        dispatcher.Invoke(() => executed = true);

        executed.ShouldBeTrue();
    }

    [Fact]
    public void Scheduler_is_the_immediate_scheduler()
    {
        var dispatcher = new ImmediateThreadDispatcher();

        dispatcher.Scheduler.ShouldBeSameAs(ImmediateScheduler.Instance);
    }

    [Fact]
    public void Invoke_rejects_null_action()
    {
        var dispatcher = new ImmediateThreadDispatcher();

        Should.Throw<ArgumentNullException>(() => dispatcher.Invoke(null!));
    }
}
