using Microsoft.JSInterop;
using BlazorMonaco;
using YamlDotNet.Core.Tokens;

namespace KubeUI.Core.Components;

public partial class KubeMonacoDiff : IDisposable
{
    [Parameter] public string Language { get; set; } = "yaml";

    [Parameter] public string Original { get; set; }

    [Parameter] public string Modified { get; set; }

    private MonacoDiffEditor _editor { get; set; }

    private DiffEditorConstructionOptions EditorConstructionOptions(MonacoDiffEditor editor)
    {
        return new DiffEditorConstructionOptions
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
                Original = await MonacoEditorBase.CreateModel(Original, Language),
                Modified = await MonacoEditorBase.CreateModel(Modified, Language)
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