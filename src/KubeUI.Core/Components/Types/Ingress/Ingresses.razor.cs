using Microsoft.AspNetCore.Components;

namespace KubeUI.Core.Components.Types
{
    [Route("/Ingresses")]
    [Route("/{Namespace}/Ingresses")]
    public partial class Ingresses
    {
        [Parameter]
        public string Namespace { get; set; }
    }
}
