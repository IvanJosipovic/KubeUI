using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace KubeUI.Core.Components.Types
{
    [Route("/CustomObject/{Group}/{Version}/{Plural}/{Name}")]
    [Route("/CustomObject/{Namespace}/{Group}/{Version}/{Plural}/{Name}")]
    public partial class CustomObject
    {
        [Inject]
        protected IState State { get; set; }

        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public string Group { get; set; }

        [Parameter]
        public string Version { get; set; }

        [Parameter]
        public string Name { get; set; }

        [Parameter]
        public string Plural { get; set; }

        private object Item;

        protected override async Task OnParametersSetAsync()
        {
            await Update();
        }

        private async Task Update()
        {
            if (Namespace == null)
            {
                Item = await State.Client.GetClusterCustomObjectAsync(Group, Version, Plural, Name);
            }
            else
            {
                Item = await State.Client.GetNamespacedCustomObjectAsync(Group, Version, Namespace, Plural, Name);
            }

            StateHasChanged();
        }
    }
}
