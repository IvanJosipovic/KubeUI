using Avalonia.Platform.Storage;

namespace KubeUI.Avalonia.Infrastructure.Platform;

public interface IPlatformServices
{
    TopLevel GetRequiredTopLevel();

    Task<bool> LaunchUriAsync(Uri uri);

    Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options);

    Task<IReadOnlyList<IStorageFolder>> OpenFolderPickerAsync(FolderPickerOpenOptions options);
}
