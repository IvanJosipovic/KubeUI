namespace KubeUI.Avalonia.Services.Icons;

public interface IResourceIconService
{
    IImage GetIcon(Type resourceType);
}
