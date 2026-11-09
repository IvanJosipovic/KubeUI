using System.Collections.Concurrent;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Threading;
using KubeUI.App.Tests.Infra;

namespace KubeUI.Benchmarks;

/// <summary>Owns one process-wide Avalonia UI thread for in-process benchmarks.</summary>
internal sealed class YamlBenchmarkUiThread : IDisposable
{
    private static readonly Lazy<YamlBenchmarkUiThread> s_instance = new(static () => new YamlBenchmarkUiThread());

    private readonly BlockingCollection<Action> _work = new();
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Thread _thread;
    private int _disposed;

    public static YamlBenchmarkUiThread Instance => s_instance.Value;

    public TestApp App { get; }

    /// <summary>Starts the dedicated Avalonia thread and waits for application startup.</summary>
    private YamlBenchmarkUiThread()
    {
        _thread = new Thread(Run)
        {
            IsBackground = true,
            Name = "KubeUI YAML benchmark UI thread",
        };
        _thread.Start();
        _ready.Task.GetAwaiter().GetResult();
        App = (TestApp)(Application.Current
            ?? throw new InvalidOperationException("Headless Avalonia application did not start."));
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
    }

    /// <summary>Runs an action on the benchmark UI thread and returns its result.</summary>
    public T Invoke<T>(Func<T> action)
    {
        if (Environment.CurrentManagedThreadId == _thread.ManagedThreadId)
        {
            return action();
        }

        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        _work.Add(() =>
        {
            try
            {
                completion.TrySetResult(action());
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }
        });

        return completion.Task.GetAwaiter().GetResult();
    }

    /// <summary>Queues an action on the benchmark UI thread and returns its completion task.</summary>
    public Task<T> InvokeAsync<T>(Func<T> action)
    {
        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        _work.Add(() =>
        {
            try
            {
                completion.TrySetResult(action());
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }
        });
        return completion.Task;
    }

    /// <summary>Stops the UI loop and releases its application and work queue.</summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
        Invoke(() =>
        {
            App.Dispose();
            return 0;
        });
        _work.CompleteAdding();
        _thread.Join();
        _work.Dispose();
    }

    private void OnProcessExit(object? sender, EventArgs eventArgs) => Dispose();

    /// <summary>Starts Avalonia and processes queued work until shutdown.</summary>
    private void Run()
    {
        try
        {
            AppBuilder.Configure<TestApp>()
                .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
                .UseSkia()
                .SetupWithoutStarting();
            _ready.TrySetResult();

            while (!_work.IsCompleted)
            {
                if (_work.TryTake(out var work, millisecondsTimeout: 1))
                {
                    work();
                }

                Dispatcher.UIThread.RunJobs();
            }
        }
        catch (Exception exception)
        {
            _ready.TrySetException(exception);
        }
    }
}
