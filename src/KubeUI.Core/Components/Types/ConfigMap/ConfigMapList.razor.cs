using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace KubeUI.Core.Components.Types
{
    public partial class ConfigMapList
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public Expression<Func<V1ConfigMap, bool>> Filter { get; set; }

        [Inject]
        protected ILogger<ConfigMapList> Logger { get; set; }

        [Inject]
        protected IState State { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private IList<V1ConfigMap> Items;

        protected override async Task OnInitializedAsync()
        {
            await Update();
        }

        private async Task Update()
        {
            IList<V1ConfigMap> items;

            if (State.Namespace?.Equals(KubeUI.Services.State.AllNameSpace) != false)
            {
                items = (await Client.ListConfigMapForAllNamespacesAsync())?.Items;
            }
            else
            {
                items = (await Client.ListNamespacedConfigMapAsync(State.Namespace))?.Items;
            }

            if (Filter != null)
            {
                items = items.AsQueryable().Where(Filter).ToList();
            }

            Items = items;
        }

        private async Task Delete(V1ConfigMap item)
        {
            await Client.DeleteNamespacedConfigMapAsync(item.Metadata.Name, item.Metadata.NamespaceProperty);

            await Update();
        }
    }
}
