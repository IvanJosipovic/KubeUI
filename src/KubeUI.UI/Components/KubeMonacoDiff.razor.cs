using Microsoft.JSInterop;
using BlazorMonaco;
using YamlDotNet.Core.Tokens;
using BlazorMonaco.Editor;

namespace KubeUI.UI.Components;

public partial class KubeMonacoDiff : IDisposable
{
    [Parameter] public string Language { get; set; } = "yaml";

    [Parameter] public string Original { get; set; }

    [Parameter] public string Modified { get; set; }

    private StandaloneDiffEditor _editor { get; set; }

    private StandaloneDiffEditorConstructionOptions EditorConstructionOptions(StandaloneDiffEditor editor)
    {
        return new StandaloneDiffEditorConstructionOptions
        {
            AutomaticLayout = true,
            IgnoreTrimWhitespace = false
        };
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            await _editor.SetModel(new DiffEditorModel()
            {
                Original = await Global.CreateModel(Original, Language),
                Modified = await Global.CreateModel(Modified, Language)
            });
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