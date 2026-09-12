namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Services;

/// <summary>
/// Saves the current contents of a pod log viewer to a user-selected file.
/// </summary>
public interface IPodLogExportService
{
    /// <summary>
    /// Opens a save picker and writes the supplied log content to the selected file.
    /// </summary>
    /// <param name="suggestedFileName">The file name initially shown by the picker.</param>
    /// <param name="content">The log text to write.</param>
    /// <param name="cancellationToken">A token used to cancel the write operation.</param>
    Task ExportAsync(string suggestedFileName, string content, CancellationToken cancellationToken = default);
}
