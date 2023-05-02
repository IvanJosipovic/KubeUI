using Microsoft.AspNetCore.Components.Rendering;

namespace KubeUI.UI.Pages
{
    [Route("/new/{Group}/{Version}/{Kind}")]
    [Route("/new/{Version}/{Kind}")]
    public partial class New : ComponentBase
    {
        [Parameter]
        public string Group { get; set; } = string.Empty;

        [Parameter]
        public string Version { get; set; }

        [Parameter]
        public string Kind { get; set; }

        private Type? ItemType { get; set; }

        private Type? ComponentType { get; set; }

        protected override void OnParametersSet()
        {
            ItemType = ModelCache.GetResourceType(Group, Version, Kind);
            ComponentType = typeof(Edit<>).MakeGenericType(new[] { ItemType });
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);

            builder.OpenComponent(1, ComponentType);
            builder.AddComponentParameter(0, nameof(Edit<V1Pod>.ShowTitle), true);
            builder.CloseComponent();
        }
    }
}