using Avalonia.Platform.Storage;

namespace KubeUI.Avalonia.Infrastructure.Platform;

public interface IPlatformServices
{
    TopLevel GetRequiredTopLevel();

    Task<bool> LaunchUriAsync(Uri uri);

    Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options);

    Task<IReadOnlyList<IStorageFolder>> OpenFolderPickerAsync(FolderPickerOpenOptions options);

    /// <summary>Shows the platform save-file picker.</summary>
    Task<IStorageFile?> SaveFilePickerAsync(FilePickerSaveOptions options);
}
