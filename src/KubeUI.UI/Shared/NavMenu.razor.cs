using KristofferStrube.Blazor.FileSystem;
using KristofferStrube.Blazor.FileSystemAccess;

namespace KubeUI.UI.Shared;

public partial class NavMenu : IDisposable
{
    [Inject]
    private ILogger<NavMenu> Logger { get; set; }

    [Inject]
    private ClusterManager ClusterManager { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    [Inject]
    private IFileSystemAccessService FileSystemAccessService { get; set; }

    private System.Timers.Timer Timer { get; set; }

    private bool OpenFolderSupported { get; set; }

    protected override async Task OnInitializedAsync()
    {
        ClusterManager.OnChange += ClusterManager_OnChange;

        Timer = new System.Timers.Timer(TimeSpan.FromSeconds(1));
        Timer.Elapsed += Timer_Elapsed;
        Timer.Enabled = true;
        Timer.AutoReset = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            OpenFolderSupported = await FileSystemAccessService.IsSupportedAsync();
        }
    }

    private async void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        await InvokeAsync(StateHasChanged);
    }

    private void ClusterManager_OnChange(ClusterManagerEvents obj)
    {
        if (obj == ClusterManagerEvents.ActiveClusterChanged)
        {
            InvokeAsync(StateHasChanged);
        }
    }

    public static string GetIcon(string iconPath)
    {
        return $"<image href=\"_content/KubeUI.UI/svg/{iconPath}\" height=\"24px\" width=\"24px\" />";
    }

    private async Task UploadFiles(InputFileChangeEventArgs e)
    {
        foreach (var file in e.GetMultipleFiles(500))
        {
            try
            {
                using var stream = new MemoryStream();
                await file.OpenReadStream(104857600).CopyToAsync(stream);
                stream.Position = 0;

                if (Path.GetExtension(file.Name) == ".zip")
                {
                    await ClusterManager.GetActiveCluster().ImportZip(stream);
                }
                else if (Path.GetExtension(file.Name) == ".yaml" || Path.GetExtension(file.Name) == ".yml")
                {
                    await ClusterManager.GetActiveCluster().ImportYaml(stream);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error parsing file: {file}", file.Name);
            }
        }
    }

    private async Task LoadFolder()
    {
        try
        {
            var folder = await FileSystemAccessService.ShowDirectoryPickerAsync();

            var values = await folder.ValuesAsync();

            var queue = new Queue<(Entity entity, FileSystemDirectoryHandle dir, FileSystemHandle value)>();
            for (var i = 0; i < values.Count(); i++)
            {
                var value = values[i] as FileSystemHandle;
                var entity = new Entity(await value.GetKindAsync(), value);
                queue.Enqueue((entity, folder, value));
            }

            while (queue.Count > 0)
            {
                var (entity, dir, value) = queue.Dequeue();
                if (await value.GetKindAsync() is FileSystemHandleKind.File)
                {
                    var fileSystemHandle = await dir.GetFileHandleAsync(await value.GetNameAsync());
                    var file = await fileSystemHandle.GetFileAsync();
                    var fileName = await fileSystemHandle.GetNameAsync();
                    var fileStream = await file.StreamAsync();

                    if (fileName.EndsWith(".yaml") || fileName.EndsWith(".yml"))
                    {
                        using var stream = new MemoryStream();
                        await fileStream.CopyToAsync(stream);
                        stream.Position = 0;

                        await ClusterManager.GetActiveCluster().ImportYaml(stream);
                    }
                }
                else
                {
                    var fileSystemDirectoryHandle = await dir.GetDirectoryHandleAsync(await value.GetNameAsync());
                    var innerValues = await fileSystemDirectoryHandle.ValuesAsync();
                    foreach (var innerValue in innerValues)
                    {
                        var innerVal2 = innerValue as FileSystemHandle;
                        var innerEntity = new Entity(await innerValue.GetKindAsync(), innerVal2);
                        entity.Children.Add(innerEntity);
                        queue.Enqueue((innerEntity, fileSystemDirectoryHandle, innerVal2));
                    }
                }
            }
        }
        catch (JSException ex) when (ex.Message.Equals("The user aborted a request.\nError: The user aborted a request.")) { }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading Directory");
        }
    }

    protected class Entity
    {
        public Entity(FileSystemHandleKind Kind, FileSystemHandle? Handle)
        {
            this.Kind = Kind;
            this.Handle = Handle;
            Children = new();
        }
        public FileSystemHandleKind Kind { get; set; }
        public FileSystemHandle? Handle { get; set; }
        public List<Entity> Children { get; set; }
    }

    private void SetActiveCluster(ICluster cluster)
    {
        ClusterManager.SetActiveCluster(cluster);

        NavigationManager.NavigateTo("/Connect");
    }

    public void Dispose()
    {
        ClusterManager.OnChange -= ClusterManager_OnChange;
        Timer.Dispose();
    }

    private bool ClustersExpanded { get; set; } = true;
    private bool WorkloadsExpanded { get; set; }
    private bool ConfigurationExpanded { get; set; }
    private bool NetworkExpanded { get; set; }
    private bool StorageExpanded { get; set; }
    private bool AccessControlExpanded { get; set; }
    private bool CRDExpanded { get; set; }
}