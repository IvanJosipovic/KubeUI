using Microsoft.AspNetCore.Components;

namespace KubeUI.Core.Components.Types
{
    [Route("/Deployments")]
    [Route("/{Namespace}/Deployments")]
    public partial class Deployments
    {
        [Parameter]
        public string Namespace { get; set; }
    }
}
