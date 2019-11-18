using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace KubeUI2.Components.Types
{
    [Route("/{Namespace}/Service/{Name}")]
    public partial class Service : IDisposable
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public string Name { get; set; }

        [Inject]
        protected IState state { get; set; }

        [Inject]
        protected IKubernetes client { get; set; }

        private V1Service Item;

        PropertyChangedEventHandler handler;

        protected override async Task OnInitializedAsync()
        {
            handler = async (xo, e) =>
            {
                if (e.PropertyName == State.UILevelNotification || e.PropertyName == State.NamespaceNotification)
                {
                    await Update();
                }
            };

            state.PropertyChanged += handler;

            await Update();
        }

        public void Dispose()
        {
            state.PropertyChanged -= handler;
        }

        private async Task Update()
        {
            Item = await client.ReadNamespacedServiceAsync(Name, Namespace);

            StateHasChanged();
        }
    }
}
