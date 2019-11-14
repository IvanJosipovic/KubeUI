using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace KubeUI2
{
    public partial class DeploymentList : IDisposable
    {
        [Parameter]
        public string Namespace { get; set; }

        [Inject]
        protected IState State { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private IList<V1Deployment> Items = new List<V1Deployment>();

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

            if (State.Namespace == null || State.Namespace.Equals(KubeUI.Services.State.AllNameSpace))
            {
                Items = (await Client.ListDeploymentForAllNamespacesAsync())?.Items;
            }
            else
            {
                Items = (await Client.ListNamespacedDeploymentAsync(State.Namespace))?.Items;
            }

            StateHasChanged();
        }
    }
}
