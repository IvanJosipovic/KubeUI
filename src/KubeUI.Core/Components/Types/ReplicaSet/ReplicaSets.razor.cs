using Microsoft.AspNetCore.Components;

namespace KubeUI.Core.Components.Types
{
    [Route("/ReplicaSets")]
    [Route("/{Namespace}/ReplicaSets")]
    public partial class ReplicaSets
    {
        [Parameter]
        public string Namespace { get; set; }
    }
}
