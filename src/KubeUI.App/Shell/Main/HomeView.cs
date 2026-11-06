using Avalonia.Markup.Xaml.MarkupExtensions;
using KubeUI.App.Infrastructure;
using KubeUI.App.Infrastructure.Presentation;
using KubeUI.App.Styles;


namespace KubeUI.App.Shell.Main;

public sealed class HomeView() : KubeUIView<HomeViewModel>
{
    protected override object Build(HomeViewModel vm) =>
        new StackPanel()
            .Children(
                new Image()
                    .Width(128)
                    .Height(128)
                    .Margin(0, 15, 0, 0)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Source(ApplicationIcons.CreateControlPlaneImage()),
                new TextBlock()
                    .Margin(0, 5, 0, 0)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .FontSize(new DynamicResourceExtension(Typography.TitleFontSizeResourceKey))
                    .Text(L10n.Instance.HomeView_Header.ToBinding()));
}
