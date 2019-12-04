using Microsoft.AspNetCore.Components;

namespace KubeUI.Core.Components.Types
{
    [Route("/Pods")]
    [Route("/{Namespace}/Pods")]
    public partial class Pods
    {
        [Parameter]
        public string Namespace { get; set; }
    }
}
