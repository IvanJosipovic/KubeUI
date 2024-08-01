using KubeUI.ViewModels;

namespace KubeUI.Views;

public sealed class AboutView : MyViewBase<AboutViewModel>
{
    protected override object Build(AboutViewModel vm) =>
        new StackPanel()
            .Children([
                new TextBlock()
                    .Padding(5,0,0,0)
                    .Text(Assets.Resources.AboutView_Header),
                new HyperlinkButton()
                    .SetProp(HyperlinkButton.NavigateUriProperty, new Uri("mailto:admin@KubeUI.com"))
                    .Content("admin@KubeUI.com"),
                new TextBlock()
                    .Padding(5,0,0,0)
                    .Text(string.Format(Assets.Resources.AboutView_Version_StringFormat, vm.Version)),
                new HyperlinkButton()
                    .SetProp(HyperlinkButton.NavigateUriProperty, new Uri("https://KubeUI.com"))
                    .Content("https://KubeUI.com"),
                ]);
}
