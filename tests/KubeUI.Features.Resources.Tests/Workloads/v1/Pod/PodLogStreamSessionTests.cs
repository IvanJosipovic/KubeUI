using KubeUI.Features.Resources.Workloads.v1.Pod.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace KubeUI.Features.Resources.Tests.Workloads.v1.Pod;

public sealed class PodLogStreamSessionTests
{
    [Fact]
    public void Dispose_should_cancel_session_and_dispose_registered_readers()
    {
        var session = CreateSession();
        session.SetReaderCount(1);
        MemoryStream stream = new();
        using var reader = new StreamReader(stream);
        var cancellationToken = session.CancellationToken;

        session.TryRegister(reader).ShouldBeTrue();
        session.Dispose();

        cancellationToken.IsCancellationRequested.ShouldBeTrue();
        stream.CanRead.ShouldBeFalse();
        session.RemainingReaders.ShouldBe(1);
        session.CompleteReader().ShouldBeTrue();
        session.Dispose();
    }

    [Fact]
    public void TryRegister_should_reject_reader_for_stale_session()
    {
        using var session = CreateSession(isCurrent: false);
        session.SetReaderCount(1);
        MemoryStream stream = new();
        using StreamReader reader = new(stream);

        session.TryRegister(reader).ShouldBeFalse();
        session.CompleteReader().ShouldBeTrue();
        stream.CanRead.ShouldBeTrue();
        reader.Dispose();
        stream.CanRead.ShouldBeFalse();
    }

    private static PodLogStreamSession CreateSession(bool isCurrent = true)
    {
        return new PodLogStreamSession(
            null!,
            null!,
            NullLogger<PodLogsViewModel>.Instance,
            new TestSink(isCurrent),
            static (_, _) => Task.CompletedTask);
    }

    private sealed class TestSink : IPodLogStreamSessionSink
    {
        private readonly bool _isCurrent;

        public TestSink(bool isCurrent)
        {
            _isCurrent = isCurrent;
        }

        public bool IsCurrent(PodLogStreamSession session) => _isCurrent;

        public bool IsReconnectPending => false;

        public int OutputGeneration => 0;

        public bool HasExistingOutput(PodLogReadOptions option) => false;

        public List<string> GetRecentMessages(PodLogReadOptions option) => [];

        public Task AppendOutputEntriesAsync(
            PodLogStreamSession session,
            IReadOnlyList<PodLogOutputEntry> entries,
            int outputGeneration) => Task.CompletedTask;

        public void AppendStatusLine(PodLogStreamSession session, PodLogOutputEntry entry, int outputGeneration)
        {
        }

        public void OpenFailed(PodLogStreamSession session, PodLogReadOptions option, Exception exception)
        {
        }

        public void ReadFailed(PodLogStreamSession session, Exception exception)
        {
        }

        public void Disconnected(PodLogStreamSession session)
        {
        }

        public void RequestReconnect(PodLogStreamSession session)
        {
        }
    }
}
