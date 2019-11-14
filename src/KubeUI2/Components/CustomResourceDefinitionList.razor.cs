using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace KubeUI2.Components
{
    public partial class CustomResourceDefinitionList : IDisposable
    {
        [Inject]
        protected IState State { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private IList<V1CustomResourceDefinition> Items { get; set; } = new List<V1CustomResourceDefinition>();

        private PropertyChangedEventHandler handler;

        protected override async Task OnInitializedAsync()
        {
            handler = async (xo, e) =>
            {
                if (e.PropertyName == KubeUI.Services.State.UILevelNotification || e.PropertyName == KubeUI.Services.State.NamespaceNotification)
                {
                    await Update();
                }
            };

            State.PropertyChanged += handler;

            await Update();
        }

        public void Dispose()
        {
            State.PropertyChanged -= handler;
        }

        private async Task Update()
        {
            Items = null;

            StateHasChanged();

            Items = (await Client.ListCustomResourceDefinitionAsync())?.Items;

            StateHasChanged();
        }

        public async Task Delete(V1CustomResourceDefinition crd)
        {
            await Client.DeleteCustomResourceDefinitionAsync(crd.Metadata.Name);
            await Update();
        }
    }
}
