using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace KubeUI2.Pages
{
    [Route("/CustomResourceDefinitions")]
    public partial class CustomResourceDefinitions
    {
        [Inject] protected ILogger<Pod> Logger { get; set; }

        [Inject] protected IState State { get; set; }

        [Inject] protected IKubernetes Client { get; set; }

        protected override async Task OnInitializedAsync()
        {
        }
    }
}
