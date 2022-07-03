namespace KubeUI.Core.Components
{
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

            if (!(await dialog.Result).Cancelled)
            {
                await ClusterManager.GetActiveCluster().Delete<TItem>(Item);
            }
        }
    }
}