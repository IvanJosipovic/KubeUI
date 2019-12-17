using Microsoft.AspNetCore.Components;

namespace KubeUI.Core.Components.Types
{
    [Route("/Secrets")]
    [Route("/{Namespace}/Secrets")]
    public partial class Secrets
    {
        [Parameter]
        public string Namespace { get; set; }
    }
}
