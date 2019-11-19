using Microsoft.AspNetCore.Components;

namespace KubeUI2.Components.Types
{
    [Route("/Ingresses")]
    [Route("/{Namespace}/Ingresses")]
    public partial class Ingresses
    {
        [Parameter]
        public string Namespace { get; set; }
    }
}
