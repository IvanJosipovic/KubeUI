using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Shouldly;
using System.Reflection;
using System.Text;

namespace KubeUI.Avalonia.Tests.Features.Workloads.Pod;

public sealed class PodLogsTests
{
    [AvaloniaFact]
    public async Task log_reader_observes_stream_shutdown_exceptions()
    {
        var logger = Application.Current.GetTestServices().GetRequiredService<ILogger<PodLogsViewModel>>();
        using PodLogsViewModel viewModel = new(logger);
        using var stream = new ThrowingReadStream(new InvalidOperationException("HTTP/2 stream completed."));

        await viewModel.ReadLogStreamForTesting(stream);

        stream.ReadCallCount.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task log_reader_stops_at_end_of_stream()
    {
        var logger = Application.Current.GetTestServices().GetRequiredService<ILogger<PodLogsViewModel>>();
        using PodLogsViewModel viewModel = new(logger);
        using var stream = new MemoryStream();

        await viewModel.ReadLogStreamForTesting(stream);
    }

    [AvaloniaFact]
    public async Task log_reader_keeps_only_the_newest_entries()
    {
        var logger = Application.Current.GetTestServices().GetRequiredService<ILogger<PodLogsViewModel>>();
        using PodLogsViewModel viewModel = new(logger);
        var content = string.Join(Environment.NewLine, Enumerable.Range(0, 10_001).Select(index => $"line-{index}"));
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        await viewModel.ReadLogStreamForTesting(stream);

        viewModel.Logs.Text.ShouldNotContain("line-0");
        viewModel.Logs.Text.ShouldContain("line-10000");
        viewModel.Logs.LineCount.ShouldBeLessThanOrEqualTo(10_000);
    }

    [AvaloniaFact]
    public async Task log_reader_does_not_create_undo_history()
    {
        var logger = Application.Current.GetTestServices().GetRequiredService<ILogger<PodLogsViewModel>>();
        using PodLogsViewModel viewModel = new(logger);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("line"));

        await viewModel.ReadLogStreamForTesting(stream);

        viewModel.Logs.UndoStack.CanUndo.ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task disposing_clears_and_detaches_the_log_document()
    {
        var logger = Application.Current.GetTestServices().GetRequiredService<ILogger<PodLogsViewModel>>();
        using PodLogsViewModel viewModel = new(logger);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("line"));
        var previousLogs = viewModel.Logs;

        await viewModel.ReadLogStreamForTesting(stream);
        previousLogs.Text.ShouldBe("line" + Environment.NewLine);

        viewModel.Dispose();

        previousLogs.Text.ShouldBeEmpty();
        viewModel.Logs.ShouldNotBeSameAs(previousLogs);
        viewModel.Logs.Text.ShouldBeEmpty();
        viewModel.Logs.UndoStack.CanUndo.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void unloading_disposes_textmate_installation()
    {
        var logger = Application.Current.GetTestServices().GetRequiredService<ILogger<PodLogsViewModel>>();
        using PodLogsViewModel viewModel = new(logger);
        var view = new PodLogsView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(content: view);
        var installationField = typeof(PodLogsView).GetField("_textMateInstallation", BindingFlags.Instance | BindingFlags.NonPublic);

        installationField.ShouldNotBeNull();

        window.Show();
        Dispatcher.UIThread.RunJobs();

        installationField!.GetValue(view).ShouldNotBeNull();

        window.Content = null;
        Dispatcher.UIThread.RunJobs();

        installationField.GetValue(view).ShouldBeNull();

        window.Content = view;
        Dispatcher.UIThread.RunJobs();

        installationField.GetValue(view).ShouldNotBeNull();
    }

    private sealed class ThrowingReadStream(Exception exception) : Stream
    {
        private readonly Exception _exception = exception;

        public int ReadCallCount { get; private set; }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => 0;
        public override long Position { get; set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ReadCallCount++;
            throw _exception;
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ReadCallCount++;
            return ValueTask.FromException<int>(_exception);
        }

        public override void Flush()
        {
        }

        public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
