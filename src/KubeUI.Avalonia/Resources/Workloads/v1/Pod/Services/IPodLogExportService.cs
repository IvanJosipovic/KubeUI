namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Services;

public interface IPodLogExportService
{
    Task ExportAsync(string suggestedFileName, string content, CancellationToken cancellationToken = default);
}
