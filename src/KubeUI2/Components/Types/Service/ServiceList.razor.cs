using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace KubeUI2.Components.Types
{
    public partial class ServiceList
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public Expression<Func<V1Service, bool>> Filter { get; set; }

        [Inject]
        protected ILogger<ServiceList> Logger { get; set; }

        [Inject]
        protected IState State { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private IList<V1Service> Items;

        protected override async Task OnInitializedAsync()
        {
            await Update();
        }

        private async Task Update()
        {
            IList<V1Service> items;

            if (State.Namespace?.Equals(KubeUI.Services.State.AllNameSpace) != false)
            {
                items = (await Client.ListServiceForAllNamespacesAsync())?.Items;
            }
            else
            {
                items = (await Client.ListNamespacedServiceAsync(State.Namespace))?.Items;
            }

            if (Filter != null)
            {
                items = items.AsQueryable().Where(Filter).ToList();
            }

            Items = items;
        }

        private async Task Delete(V1Service item)
        {
            await Client.DeleteNamespacedServiceAsync(item.Metadata.Name, item.Metadata.NamespaceProperty);

            await Update();
        }
    }
}
