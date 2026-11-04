using System.Diagnostics;
using Avalonia.Threading;
using k8s;

namespace KubeUI.App.Infrastructure;

/// <summary>Provides the process-wide error events consumed by the application.</summary>
internal interface IApplicationEventSource
{
    event UnhandledExceptionEventHandler? CurrentDomainUnhandledException;
    event DispatcherUnhandledExceptionEventHandler? DispatcherUnhandledException;
    event EventHandler<UnobservedTaskExceptionEventArgs>? UnobservedTaskException;
    event EventHandler<DataReceivedEventArgs>? KubernetesExecStdError;
}

/// <summary>Forwards process-wide framework and Kubernetes client error events.</summary>
internal sealed class ApplicationEventSource : IApplicationEventSource
{
    public event UnhandledExceptionEventHandler? CurrentDomainUnhandledException
    {
        add => AppDomain.CurrentDomain.UnhandledException += value;
        remove => AppDomain.CurrentDomain.UnhandledException -= value;
    }

    public event DispatcherUnhandledExceptionEventHandler? DispatcherUnhandledException
    {
        add => Dispatcher.UIThread.UnhandledException += value;
        remove => Dispatcher.UIThread.UnhandledException -= value;
    }

    public event EventHandler<UnobservedTaskExceptionEventArgs>? UnobservedTaskException
    {
        add => TaskScheduler.UnobservedTaskException += value;
        remove => TaskScheduler.UnobservedTaskException -= value;
    }

    public event EventHandler<DataReceivedEventArgs>? KubernetesExecStdError
    {
        add => KubernetesClientConfiguration.ExecStdError += value;
        remove => KubernetesClientConfiguration.ExecStdError -= value;
    }
}
