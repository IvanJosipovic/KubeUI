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

namespace KubeUI.Core.Components.Types
{
    public partial class IngressList
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public Expression<Func<Extensionsv1beta1Ingress, bool>> Filter { get; set; }

        [Inject]
        protected ILogger<IngressList> Logger { get; set; }

        [Inject]
        protected IState State { get; set; }

        [Inject]
        protected IKubernetes Client { get; set; }

        private IList<Extensionsv1beta1Ingress> Items;

        protected override async Task OnInitializedAsync()
        {
            await Update();
        }

        private async Task Update()
        {
            IList<Extensionsv1beta1Ingress> items;

            if (State.Namespace?.Equals(KubeUI.Services.State.AllNameSpace) != false)
            {
                items = (await Client.ListIngressForAllNamespacesAsync())?.Items;
            }
            else
            {
                items = (await Client.ListNamespacedIngressAsync(State.Namespace))?.Items;
            }

            if (Filter != null)
            {
                items = items.AsQueryable().Where(Filter).ToList();
            }

            Items = items;
        }

        private async Task Delete(Extensionsv1beta1Ingress item)
        {
            await Client.DeleteNamespacedServiceAsync(item.Metadata.Name, item.Metadata.NamespaceProperty);

            await Update();
        }
    }
}
