using Microsoft.AspNetCore.Components;

namespace KubeUI.Core.Components.Types
{
    [Route("/{Group}/{Version}/{Plural}/CustomObjects")]
    [Route("/{Namespace}/{Group}/{Version}/{Plural}/CustomObjects")]
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
