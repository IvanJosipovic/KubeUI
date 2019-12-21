using Microsoft.AspNetCore.Components;

namespace KubeUI.Core.Components.Types
{
    [Route("/ConfigMaps")]
    [Route("/{Namespace}/ConfigMaps")]
    public partial class ConfigMaps
    {
        [Parameter]
        public string Namespace { get; set; }
    }
}
