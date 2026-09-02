using System.Text;
using Avalonia.Platform.Storage;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Services;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Resources.Workloads.v1.Pod;

public sealed class PodLogExportServiceTests
{
    [Fact]
    public async Task ExportAsync_writes_utf8_content_to_the_selected_file()
    {
        MemoryStream output = new();
        Mock<IStorageFile> file = new(MockBehavior.Strict);
        file.Setup(item => item.OpenWriteAsync()).ReturnsAsync(output);
        FilePickerSaveOptions? capturedOptions = null;
        Mock<IPlatformServices> platform = new(MockBehavior.Strict);
        platform.Setup(item => item.SaveFilePickerAsync(It.IsAny<FilePickerSaveOptions>()))
            .Callback<FilePickerSaveOptions>(options => capturedOptions = options)
            .ReturnsAsync(file.Object);
        PodLogExportService service = new(platform.Object);

        await service.ExportAsync("api.log", "first\nsecond");

        Encoding.UTF8.GetString(output.ToArray()).ShouldBe("first\nsecond");
        capturedOptions.ShouldNotBeNull();
        capturedOptions.Title.ShouldBe(KubeUI.Avalonia.Assets.Resources.PodLogsView_Download);
        capturedOptions.SuggestedFileName.ShouldBe("api.log");
        capturedOptions.FileTypeChoices.ShouldHaveSingleItem()
            .Patterns.ShouldBe(["*.log", "*.txt"]);
        platform.VerifyAll();
        file.VerifyAll();
        output.Dispose();
    }

    [Fact]
    public async Task ExportAsync_returns_when_the_picker_is_cancelled()
    {
        Mock<IPlatformServices> platform = new(MockBehavior.Strict);
        platform.Setup(item => item.SaveFilePickerAsync(It.IsAny<FilePickerSaveOptions>()))
            .ReturnsAsync((IStorageFile?)null);
        PodLogExportService service = new(platform.Object);

        await service.ExportAsync("api.log", "content");

        platform.VerifyAll();
    }

    [Fact]
    public async Task ExportAsync_honors_cancellation_before_showing_the_picker()
    {
        Mock<IPlatformServices> platform = new(MockBehavior.Strict);
        PodLogExportService service = new(platform.Object);
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(
            () => service.ExportAsync("api.log", "content", cancellation.Token));

        platform.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExportAsync_honors_cancellation_after_the_picker_closes()
    {
        TaskCompletionSource<IStorageFile?> picker =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        Mock<IStorageFile> file = new(MockBehavior.Strict);
        Mock<IPlatformServices> platform = new(MockBehavior.Strict);
        platform.Setup(item => item.SaveFilePickerAsync(It.IsAny<FilePickerSaveOptions>()))
            .Returns(picker.Task);
        PodLogExportService service = new(platform.Object);
        using CancellationTokenSource cancellation = new();

        Task exportTask = service.ExportAsync("api.log", "content", cancellation.Token);
        await cancellation.CancelAsync();
        picker.SetResult(file.Object);

        await Should.ThrowAsync<OperationCanceledException>(() => exportTask);
        file.VerifyNoOtherCalls();
        platform.VerifyAll();
    }
}
