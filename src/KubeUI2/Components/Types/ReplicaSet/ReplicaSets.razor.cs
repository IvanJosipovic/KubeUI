using Microsoft.AspNetCore.Components;

namespace KubeUI2.Components.Types
{
    [Route("/ReplicaSets")]
    [Route("/{Namespace}/ReplicaSets")]
    public partial class ReplicaSets
    {
        [Parameter]
        public string Namespace { get; set; }
    }
}
