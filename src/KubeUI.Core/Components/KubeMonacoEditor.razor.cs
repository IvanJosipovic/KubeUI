using Microsoft.JSInterop;
using BlazorMonaco;

namespace KubeUI.Core.Components;

public partial class KubeMonacoEditor : IDisposable
{
    [Parameter]
    public string Language { get; set; } = "yaml";

    [Parameter]
    public string Value { get; set; }

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    [Parameter]
    public bool ReadOnly { get; set; }

    private MonacoEditor _editor { get; set; }

    private StandaloneEditorConstructionOptions EditorConstructionOptions(MonacoEditor editor)
    {
        return new StandaloneEditorConstructionOptions
        {
            Language = Language,
            Value = Value,
            AutomaticLayout = true,
            ReadOnly = ReadOnly
        };
    }

    string oldValue;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (!Value.Equals(oldValue))
            {
                await _editor.SetValue(Value);
                oldValue = Value;
            }
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

    private async void OnDidBlurr(MonacoEditor editor)
    {
        var yaml = await editor.GetValue();
        await ValueChanged.InvokeAsync(yaml);
    }
}