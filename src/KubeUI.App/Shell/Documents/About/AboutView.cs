using Avalonia.Markup.Xaml.MarkupExtensions;
using KubeUI.App.Infrastructure;
using KubeUI.App.Infrastructure.Presentation;
using KubeUI.App.Styles;


namespace KubeUI.App.Shell.Documents.About;

public sealed class AboutView() : KubeUIView<AboutViewModel>()
{
    protected override object Build(AboutViewModel vm) =>
        new StackPanel()
            .Children(
                new Image()
                    .Width(128)
                    .Height(128)
                    .Margin(0, 15, 0, 5)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Source(ApplicationIcons.CreateControlPlaneImage()),
                new TextBlock()
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .FontSize(new DynamicResourceExtension(Typography.TitleFontSizeResourceKey))
                    .Text(SharedL10n.Instance.MainWindow_Title.ToBinding()),
                new TextBlock()
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Text(string.Format(SharedL10n.Instance.AboutView_Version_StringFormat.CurrentValue()!, vm.Version ?? SharedL10n.Instance.AboutView_Version_FallbackValue.CurrentValue()!)),
                new TextBlock()
                    .Padding(0, 15, 0, 0)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Text(SharedL10n.Instance.AboutView_CreatedBy.ToBinding()),
                new StackPanel()
                    .Margin(0, 15, 0, 0)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Orientation(Orientation.Horizontal)
                    .Children(
                        new TextBlock()
                            .Text(SharedL10n.Instance.AboutView_Contact.ToBinding()),
                        new HyperlinkButton()
                            .Padding(2, -2, 0, 0)
                            .Content(SharedL10n.Instance.AboutView_ContactEmailContent.ToBinding())
                            .NavigateUri(new Uri(SharedL10n.Instance.AboutView_ContactEmailUri.CurrentValue()!))),
                new StackPanel()
                    .Margin(0, 5, 0, 0)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Orientation(Orientation.Horizontal)
                    .Children(
                        new TextBlock()
                            .Text(SharedL10n.Instance.AboutView_Website.ToBinding()),
                        new HyperlinkButton()
                            .Padding(2, -2, 0, 0)
                            .Content(SharedL10n.Instance.AboutView_WebsiteContent.ToBinding())
                            .NavigateUri(new Uri(SharedL10n.Instance.AboutView_WebsiteUri.CurrentValue()!))));
}
