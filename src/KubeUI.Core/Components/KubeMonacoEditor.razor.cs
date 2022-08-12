using Microsoft.JSInterop;
using BlazorMonaco;

namespace KubeUI.Core.Components
{
    public partial class KubeMonacoEditor : IDisposable
    {
        [Parameter]
        public string Language { get; set; } = "yaml";

        [Parameter]
        public string Value { get; set; }

        [Parameter]
        public bool ReadOnly { get; set; }

        private MonacoEditor _editor { get; set; }

        private StandaloneEditorConstructionOptions EditorConstructionOptions(MonacoEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                Language = Language,
                GlyphMargin = true,
                Value = Value,
                AutomaticLayout = true,
                ReadOnly = ReadOnly
            };
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            try
            {
                await _editor.SetValue(Value);
            }
            catch (JSException)
            {
            }
        }

        protected override void OnParametersSet()
        {
            StateHasChanged();
        }

        public void Dispose()
        {
            _editor?.DisposeEditor();
            _editor?.Dispose();
        }
    }
}