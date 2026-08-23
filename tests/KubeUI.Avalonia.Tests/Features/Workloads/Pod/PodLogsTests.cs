using Avalonia.Headless.XUnit;
using Microsoft.Extensions.Logging;
using Shouldly;

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
