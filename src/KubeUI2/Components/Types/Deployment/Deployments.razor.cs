using Microsoft.AspNetCore.Components;

namespace KubeUI2.Components.Types
{
    [Route("/Deployments")]
    [Route("/{Namespace}/Deployments")]
    public partial class Deployments
    {
        [Parameter]
        public string Namespace { get; set; }
    }
}
