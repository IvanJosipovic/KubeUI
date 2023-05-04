namespace KubeUI.UI.Components;

public partial class Actions<TItem>
{
    [Parameter]
    public TItem Item { get; set; }

    [Parameter]
    public HashSet<TItem> SelectedItems { get; set; }

    [Inject]
    private IDialogService Dialog { get; set; }

    [Inject]
    private ClusterManager ClusterManager { get; set; }

    [Inject]
    private ISnackbar Snackbar { get; set; }

    private async Task Delete()
    {
        var parameters = new DialogParameters()
        {
            { "ContentText", $"Do you want to delete {Item.Name()}?" },
            { "ButtonText", "Delete" }, { "Color", Color.Error }
        };
        var dialog = Dialog.Show<Dialog>("Delete", parameters, new DialogOptions()
        {
            CloseButton = true
        });

        if (!(await dialog.Result).Canceled)
        {
            try
            {
                await ClusterManager.GetActiveCluster().Delete(Item);
            }
            catch (Exception ex)
            {
                Snackbar.Add("Failed Delete Resource: " + ex.Message, Severity.Error);
            }
        }
    }
}