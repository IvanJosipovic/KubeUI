using System.Text;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using KubeUI.Avalonia.Infrastructure.Platform;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Services;

/// <inheritdoc />
public sealed class PodLogExportService(IPlatformServices platformServices) : IPodLogExportService
{
    /// <inheritdoc />
    public async Task ExportAsync(string suggestedFileName, string content, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        FilePickerSaveOptions options = new()
        {
            Title = global::KubeUI.Avalonia.Assets.Resources.PodLogsView_Download,
            SuggestedFileName = suggestedFileName,
            FileTypeChoices =
            [
                new FilePickerFileType(global::KubeUI.Avalonia.Assets.Resources.PodLogsView_FileTypeText)
                {
                    Patterns = ["*.log", "*.txt"],
                },
            ],
        };

        var file = await platformServices.SaveFilePickerAsync(options);
        cancellationToken.ThrowIfCancellationRequested();
        if (file is null)
        {
            return;
        }

        await using var stream = await file.OpenWriteAsync();
        await using StreamWriter writer = new(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        await writer.WriteAsync(content.AsMemory(), cancellationToken);
        await writer.FlushAsync(cancellationToken);
    }
}
