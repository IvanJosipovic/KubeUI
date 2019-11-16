using Microsoft.AspNetCore.Components;

namespace KubeUI2.Components.Types
{
    [Route("/Pods")]
    [Route("/{Namespace}/Pods")]
    public partial class Pods
    {
        [Parameter]
        public string Namespace { get; set; }
    }
}
