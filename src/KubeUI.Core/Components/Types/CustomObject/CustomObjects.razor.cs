using Microsoft.AspNetCore.Components;

namespace KubeUI.Core.Components.Types
{
    [Route("/CustomObjects/{Group}/{Version}/{Plural}")]
    [Route("/CustomObjects/{Namespace}/{Group}/{Version}/{Plural}")]
    public partial class CustomObjects
    {
        [Parameter]
        public string Namespace { get; set; }

        [Parameter]
        public string Group { get; set; }

        [Parameter]
        public string Version { get; set; }

        [Parameter]
        public string Plural { get; set; }
    }
}
