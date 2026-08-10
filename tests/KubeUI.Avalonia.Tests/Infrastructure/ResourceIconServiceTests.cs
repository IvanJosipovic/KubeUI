using Avalonia.Headless.XUnit;
using Avalonia.Svg.Skia;
using k8s.Models;
using KubeUI.Avalonia.Services.Icons;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure;

public sealed class ResourceIconServiceTests
{
    [AvaloniaFact]
    public void Known_resource_type_returns_mapped_icon()
    {
        ResourceIconService service = new();

        var icon = service.GetIcon(typeof(V1Pod));

        var svgIcon = icon.ShouldBeOfType<SvgImage>();
        svgIcon.Source.ShouldNotBeNull();
        svgIcon.Source!.Path.ShouldBe("/Assets/kube/resources/unlabeled/pod.svg");
    }

    [AvaloniaFact]
    public void Unknown_resource_type_returns_generated_icon()
    {
        ResourceIconService service = new();

        var icon = service.GetIcon(typeof(V1Alertmanager));

        var svgIcon = icon.ShouldBeOfType<SvgImage>();
        svgIcon.Source.ShouldNotBeNull();
        svgIcon.Source!.Path.ShouldBeNull();
        svgIcon.Source.Picture.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public void Repeated_resource_type_requests_reuse_parsed_svg_source()
    {
        ResourceIconService service = new();

        var first = service.GetIcon(typeof(V1Alertmanager));
        var second = service.GetIcon(typeof(V1Alertmanager));

        first.ShouldNotBeSameAs(second);
        first.ShouldBeOfType<SvgImage>().Source.ShouldBeSameAs(second.ShouldBeOfType<SvgImage>().Source);
    }

    private sealed class V1Alertmanager
    {
        public const string KubeKind = "Alertmanager";
    }
}
