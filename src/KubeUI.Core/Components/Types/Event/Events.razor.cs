using Microsoft.AspNetCore.Components;

namespace KubeUI.Core.Components.Types
{
    [Route("/Events")]
    [Route("/{Namespace}/Events")]
    public partial class Events
    {
        [Parameter]
        public string Namespace { get; set; }
    }
}
