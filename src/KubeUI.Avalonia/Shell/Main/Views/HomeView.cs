using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Svg.Skia;
using KubeUI.Avalonia.Shell.Main.ViewModels;

namespace KubeUI.Avalonia.Shell.Main.Views;

public sealed class HomeView() : ViewBase<HomeViewModel>
{
    protected override object Build(HomeViewModel vm) =>
        new StackPanel()
            .Children(
                new Image()
                    .Width(128)
                    .Height(128)
                    .Margin(0, 15, 0, 0)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Source(new SvgImage
                    {
                        Source = SvgSource.Load("/Assets/kube/infrastructure_components/unlabeled/control-plane.svg", new Uri("avares://KubeUI.Avalonia"))
                    }),
                new TextBlock()
                    .Margin(0, 5, 0, 0)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .FontSize(24)
                    .Text(Assets.Resources.HomeView_Header));
}
