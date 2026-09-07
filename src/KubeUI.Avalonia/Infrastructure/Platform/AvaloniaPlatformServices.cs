using Avalonia.Platform.Storage;

namespace KubeUI.Avalonia.Infrastructure.Platform;

public sealed class AvaloniaPlatformServices : IPlatformServices
{
    public TopLevel GetRequiredTopLevel()
    {
        return TopLevelAccessor.GetRequired();
    }

    public Task<bool> LaunchUriAsync(Uri uri)
    {
        return GetRequiredTopLevel().Launcher.LaunchUriAsync(uri);
    }

    public Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options)
    {
        return GetRequiredTopLevel().StorageProvider.OpenFilePickerAsync(options);
    }

    public Task<IReadOnlyList<IStorageFolder>> OpenFolderPickerAsync(FolderPickerOpenOptions options)
    {
        return GetRequiredTopLevel().StorageProvider.OpenFolderPickerAsync(options);
    }

    /// <inheritdoc />
    public Task<IStorageFile?> SaveFilePickerAsync(FilePickerSaveOptions options)
    {
        return GetRequiredTopLevel().StorageProvider.SaveFilePickerAsync(options);
    }
}
