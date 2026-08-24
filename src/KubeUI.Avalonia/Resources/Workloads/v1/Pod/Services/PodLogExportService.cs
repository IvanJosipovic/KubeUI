using System.Text;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using KubeUI.Avalonia.Infrastructure.Platform;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Services;

public sealed class PodLogExportService : IPodLogExportService
{
    public async Task ExportAsync(string suggestedFileName, string content, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var topLevel = TopLevelAccessor.GetRequired();
        FilePickerSaveOptions options = new()
        {
            Title = global::KubeUI.Avalonia.Assets.Resources.PodLogsView_Download,
            SuggestedFileName = suggestedFileName,
            FileTypeChoices =
            [
                new FilePickerFileType("Text files")
                {
                    Patterns = ["*.log", "*.txt"],
                },
            ],
        };

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(options);
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
