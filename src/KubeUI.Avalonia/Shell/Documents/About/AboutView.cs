using System;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Svg.Skia;

namespace KubeUI.Avalonia.Shell.Documents.About;

public sealed class AboutView() : ViewBase<AboutViewModel>()
{
    protected override object Build(AboutViewModel vm) =>
        new StackPanel()
            .Children(
                new Image()
                    .Width(128)
                    .Height(128)
                    .Margin(0, 15, 0, 5)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Source(
                        new SvgImage()
                            .Source(SvgSource.Load("avares://KubeUI.Avalonia/Assets/kube/infrastructure_components/unlabeled/control-plane.svg"))),
                new TextBlock()
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .FontSize(24)
                    .Text(Assets.Resources.MainWindow_Title),
                new TextBlock()
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Text(string.Format(Assets.Resources.AboutView_Version_StringFormat!, vm.Version ?? Assets.Resources.AboutView_Version_FallbackValue)),
                new TextBlock()
                    .Padding(0, 15, 0, 0)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Text(Assets.Resources.AboutView_CreatedBy),
                new StackPanel()
                    .Margin(0, 15, 0, 0)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Orientation(Orientation.Horizontal)
                    .Children(
                        new TextBlock()
                            .Text(Assets.Resources.AboutView_Contact),
                        new HyperlinkButton()
                            .Padding(2, -2, 0, 0)
                            .Content(Assets.Resources.AboutView_ContactEmailContent)
                            .NavigateUri(new Uri(Assets.Resources.AboutView_ContactEmailUri!))),
                new StackPanel()
                    .Margin(0, 5, 0, 0)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Orientation(Orientation.Horizontal)
                    .Children(
                        new TextBlock()
                            .Text(Assets.Resources.AboutView_Website),
                        new HyperlinkButton()
                            .Padding(2, -2, 0, 0)
                            .Content(Assets.Resources.AboutView_WebsiteContent)
                            .NavigateUri(new Uri(Assets.Resources.AboutView_WebsiteUri!))));
}
