using Microsoft.AspNetCore.Components;

namespace KubeUI.Core.Components.Types
{
    [Route("/DaemonSets")]
    [Route("/{Namespace}/DaemonSets")]
    public partial class DaemonSets
    {
        [Parameter]
        public string Namespace { get; set; }
    }
}
