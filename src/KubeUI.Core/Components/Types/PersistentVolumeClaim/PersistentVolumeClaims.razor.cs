using Microsoft.AspNetCore.Components;

namespace KubeUI.Core.Components.Types
{
    [Route("/PersistentVolumeClaims")]
    [Route("{Namespace}/PersistentVolumeClaims")]
    public partial class PersistentVolumeClaims
    {
        [Parameter]
        public string Namespace { get; set; }
    }
}
