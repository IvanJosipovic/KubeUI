using Avalonia.Platform.Storage;

namespace KubeUI.Avalonia.Infrastructure.Platform;

/// <summary>Provides platform services backed by the current Avalonia top level.</summary>
public sealed class AvaloniaPlatformServices : IPlatformServices
{
    /// <inheritdoc />
    public TopLevel GetRequiredTopLevel()
    {
        return TopLevelAccessor.GetRequired();
    }

    /// <inheritdoc />
    public Task<bool> LaunchUriAsync(Uri uri)
    {
        return GetRequiredTopLevel().Launcher.LaunchUriAsync(uri);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options)
    {
        return GetRequiredTopLevel().StorageProvider.OpenFilePickerAsync(options);
    }

    /// <inheritdoc />
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
