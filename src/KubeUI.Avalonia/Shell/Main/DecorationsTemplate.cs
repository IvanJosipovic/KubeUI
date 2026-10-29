using Avalonia.Controls.Chrome;
using Avalonia.Controls.Templates;

namespace KubeUI.Avalonia.Shell.Main;

internal sealed class DecorationsTemplate(Func<NameScope, WindowDrawnDecorationsContent> build)
    : IWindowDrawnDecorationsTemplate
{
    public TemplateResult<WindowDrawnDecorationsContent> Build()
    {
        var names = new NameScope();
        return new TemplateResult<WindowDrawnDecorationsContent>(build(names), names);
    }

    object ITemplate.Build() => Build();
}
