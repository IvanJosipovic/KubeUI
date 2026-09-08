using Avalonia.Platform.Storage;

namespace KubeUI.Avalonia.Infrastructure.Platform;

/// <summary>Provides platform integrations used by the Avalonia application.</summary>
public interface IPlatformServices
{
    /// <summary>Gets the current top-level Avalonia control.</summary>
    TopLevel GetRequiredTopLevel();

    /// <summary>Launches a URI using the platform.</summary>
    Task<bool> LaunchUriAsync(Uri uri);

    /// <summary>Shows the platform open-file picker.</summary>
    Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options);

    /// <summary>Shows the platform open-folder picker.</summary>
    Task<IReadOnlyList<IStorageFolder>> OpenFolderPickerAsync(FolderPickerOpenOptions options);

    /// <summary>Shows the platform save-file picker.</summary>
    Task<IStorageFile?> SaveFilePickerAsync(FilePickerSaveOptions options);
}
