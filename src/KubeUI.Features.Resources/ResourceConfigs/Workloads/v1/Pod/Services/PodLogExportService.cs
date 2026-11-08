using System.Text;
using Avalonia.Platform.Storage;
using KubeUI.Controls.Platform;

namespace KubeUI.Features.Resources.Workloads.v1.Pod.Services;

/// <summary>Exports pod log text through the platform save-file picker.</summary>
public sealed class PodLogExportService(IPlatformServices platformServices) : IPodLogExportService
{
    /// <inheritdoc />
    public async Task ExportAsync(string suggestedFileName, string content, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        FilePickerSaveOptions options = new()
        {
            Title = L10n.Instance.PodLogsView_Download.CurrentValue()!,
            SuggestedFileName = suggestedFileName,
            FileTypeChoices =
            [
                new FilePickerFileType(L10n.Instance.PodLogsView_FileTypeText.CurrentValue()!)
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
