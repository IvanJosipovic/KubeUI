using Microsoft.AspNetCore.Components;

namespace KubeUI2.Components.Types
{
    [Route("/Services")]
    [Route("/{Namespace}/Services")]
    public partial class Services
    {
        [Parameter]
        public string Namespace { get; set; }
    }
}
