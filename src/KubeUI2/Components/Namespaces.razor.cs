using k8s;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KubeUI2.Components
{
    public partial class Namespaces
    {
        [Inject]
        protected IState state { get; set; }

        [Inject]
        protected IKubernetes client { get; set; }

        [Inject]
        protected NavigationManager navigationManager { get; set; }

        private List<string> Options { get; set; } = new List<string>();

        protected override async Task OnInitializedAsync()
        {
            Options = (await client.ListNamespaceAsync())?.Items.Select(x => x.Metadata.Name).ToList();

            StateHasChanged();
        }

        private void OnChange(ChangeEventArgs args)
        {
            state.Namespace = args.Value.ToString();
        }
    }
}
