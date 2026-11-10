using KubeUI.Features.AI.Acp;
using KubeUI.Features.AI.Agents;
using KubeUI.Features.AI.Permissions;
using KubeUI.Testing.Utilities;
using Shouldly;
using SvcSystems.AI.AgentClientProtocol.Client.Models;

namespace KubeUI.Features.AI.Tests.Acp;

public sealed class FileSystemHandlerTests
{
    [Fact]
    public async Task ReadTextFile_returns_requested_one_based_line_window()
    {
        var path = CreateFile("line-one\nline-two\nline-three\nline-four");
        var handler = new FileSystemHandler(new ScriptedPermissionService(allowed: true));

        var content = await handler.ReadTextFileAsync(
            new ReadTextFileRequest { SessionId = "session", Path = path, Line = 2, Limit = 2 },
            TestContext.Current.CancellationToken);

        content.ShouldBe("line-two\nline-three");
    }

    [Fact]
    public async Task ReadTextFile_without_line_reads_the_whole_file()
    {
        var path = CreateFile("alpha\nbeta");
        var handler = new FileSystemHandler(new ScriptedPermissionService(allowed: true));

        var content = await handler.ReadTextFileAsync(
            new ReadTextFileRequest { SessionId = "session", Path = path },
            TestContext.Current.CancellationToken);

        content.ShouldBe("alpha\nbeta");
    }

    [Fact]
    public async Task ReadTextFile_past_the_end_of_file_returns_empty()
    {
        var path = CreateFile("only-line");
        var handler = new FileSystemHandler(new ScriptedPermissionService(allowed: true));

        var content = await handler.ReadTextFileAsync(
            new ReadTextFileRequest { SessionId = "session", Path = path, Line = 10 },
            TestContext.Current.CancellationToken);

        content.ShouldBeEmpty();
    }

    [Fact]
    public async Task ReadTextFile_denied_by_permission_service_throws()
    {
        var path = CreateFile("secret");
        var handler = new FileSystemHandler(new ScriptedPermissionService(allowed: false, reason: "read denied"));

        await Should.ThrowAsync<UnauthorizedAccessException>(async () => await handler.ReadTextFileAsync(
            new ReadTextFileRequest { SessionId = "session", Path = path },
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task WriteTextFile_writes_content()
    {
        var path = Path.Combine(Path.GetTempPath(), $"kubeui-acp-{Guid.NewGuid():N}.txt");
        try
        {
            var handler = new FileSystemHandler(new ScriptedPermissionService(allowed: true));

            await handler.WriteTextFileAsync(
                new WriteTextFileRequest { SessionId = "session", Path = path, Content = "written" },
                TestContext.Current.CancellationToken);

            File.ReadAllText(path).ShouldBe("written");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task WriteTextFile_denied_by_permission_service_throws_and_writes_nothing()
    {
        var path = Path.Combine(Path.GetTempPath(), $"kubeui-acp-{Guid.NewGuid():N}.txt");
        try
        {
            var handler = new FileSystemHandler(new ScriptedPermissionService(allowed: false));

            await Should.ThrowAsync<UnauthorizedAccessException>(async () => await handler.WriteTextFileAsync(
                new WriteTextFileRequest { SessionId = "session", Path = path, Content = "written" },
                TestContext.Current.CancellationToken));

            File.Exists(path).ShouldBeFalse();
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Paths_outside_the_configured_roots_are_rejected()
    {
        var insideRoot = Path.Combine(Path.GetTempPath(), $"kubeui-acp-root-{Guid.NewGuid():N}");
        Directory.CreateDirectory(insideRoot);
        var outsidePath = Path.Combine(Path.GetTempPath(), $"kubeui-acp-{Guid.NewGuid():N}.txt");
        try
        {
            var handler = new FileSystemHandler(
                new ScriptedPermissionService(allowed: true),
                new HashSet<string>([insideRoot], StringComparer.Ordinal));

            await Should.ThrowAsync<UnauthorizedAccessException>(async () => await handler.ReadTextFileAsync(
                new ReadTextFileRequest { SessionId = "session", Path = outsidePath },
                TestContext.Current.CancellationToken));
            await Should.ThrowAsync<UnauthorizedAccessException>(async () => await handler.WriteTextFileAsync(
                new WriteTextFileRequest { SessionId = "session", Path = outsidePath, Content = "x" },
                TestContext.Current.CancellationToken));

            var insidePath = Path.Combine(insideRoot, "allowed.txt");
            await handler.WriteTextFileAsync(
                new WriteTextFileRequest { SessionId = "session", Path = insidePath, Content = "ok" },
                TestContext.Current.CancellationToken);
            File.ReadAllText(insidePath).ShouldBe("ok");
        }
        finally
        {
            Directory.Delete(insideRoot, recursive: true);
            File.Delete(outsidePath);
        }
    }

    [Fact]
    public async Task Relative_paths_escaping_the_root_are_rejected()
    {
        var insideRoot = Path.Combine(Path.GetTempPath(), $"kubeui-acp-root-{Guid.NewGuid():N}");
        Directory.CreateDirectory(insideRoot);
        try
        {
            var handler = new FileSystemHandler(
                new ScriptedPermissionService(allowed: true),
                new HashSet<string>([insideRoot], StringComparer.Ordinal));

            await Should.ThrowAsync<UnauthorizedAccessException>(async () => await handler.ReadTextFileAsync(
                new ReadTextFileRequest { SessionId = "session", Path = Path.Combine(insideRoot, "..", "escaped.txt") },
                TestContext.Current.CancellationToken));
        }
        finally
        {
            Directory.Delete(insideRoot, recursive: true);
        }
    }

    private static string CreateFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"kubeui-acp-{Guid.NewGuid():N}.txt");
        File.WriteAllText(path, content);
        return path;
    }

    private sealed class ScriptedPermissionService(bool allowed, string? reason = null) : IAgentPermissionService
    {
        public Task<AgentPermissionResult> RequestPermissionAsync(AgentPermissionRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new AgentPermissionResult(allowed, reason));
    }
}
