using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace KubeUI.Core.Components.Types
{
    public partial class NodeList
    {
        [Parameter]
        public Expression<Func<V1Node, bool>> Filter { get; set; }

        [Inject]
        protected IState State { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private IList<V1Node> Items;

        protected override async Task OnInitializedAsync()
        {
            await Update();
        }

        private async Task Update()
        {
            IList<V1Node> items = (await Client.ListNodeAsync())?.Items;

            if (Filter != null)
            {
                items = items.AsQueryable().Where(Filter).ToList();
            }

            Items = items;
        }

        private async Task Delete(V1Node item)
        {
            await Client.DeleteNodeAsync(item.Metadata.Name);

            await Update();
        }
    }
}
