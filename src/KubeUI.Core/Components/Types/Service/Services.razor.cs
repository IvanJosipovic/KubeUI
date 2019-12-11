using Microsoft.AspNetCore.Components;

namespace KubeUI.Core.Components.Types
{
    [Route("/Services")]
    [Route("/{Namespace}/Services")]
    public partial class Services
    {
        [Parameter]
        public string Namespace { get; set; }
    }
}
