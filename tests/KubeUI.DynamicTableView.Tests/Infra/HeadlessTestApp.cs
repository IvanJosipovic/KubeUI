using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;

namespace KubeUI.DynamicTableView.Tests.Infra;

public sealed class HeadlessTestApp : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        Styles.Add(new FluentTheme());
        Styles.Add(new StyleInclude(new Uri("avares://KubeUI.DynamicTableView"))
        {
            Source = new Uri("avares://KubeUI.DynamicTableView/Themes/DynamicTableViewTheme.axaml")
        });
        base.OnFrameworkInitializationCompleted();
    }
}
