using Microsoft.AspNetCore.Components;

namespace KubeUI.Core.Components.Types
{
    [Route("/StatefulSets")]
    [Route("/{Namespace}/StatefulSets")]
    public partial class StatefulSets
    {
        [Parameter]
        public string Namespace { get; set; }
    }
}
