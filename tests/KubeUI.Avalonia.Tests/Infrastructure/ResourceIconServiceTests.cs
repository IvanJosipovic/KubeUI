using Avalonia.Headless.XUnit;
using Avalonia.Svg.Skia;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Services.Icons;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure;

public sealed class ResourceIconServiceTests
{
    [AvaloniaFact]
    public void Known_resource_type_returns_mapped_icon()
    {
        ResourceIconService service = new();

        var icon = service.GetIcon(GroupApiVersionKind.From<V1Pod>());

        var svgIcon = icon.ShouldBeOfType<SvgImage>();
        svgIcon.Source.ShouldNotBeNull();
        svgIcon.Source!.Path.ShouldBe("/Assets/kube/resources/unlabeled/pod.svg");
    }

    [AvaloniaFact]
    public void Api_group_icon_mapping_is_preserved()
    {
        AssertMappedIcon(new GroupApiVersionKind(string.Empty, "v1", "APIGroup", string.Empty), "/Assets/kube/resources/unlabeled/group.svg");
    }

    [AvaloniaFact]
    public void V1_horizontal_pod_autoscaler_icon_mapping_is_preserved()
    {
        AssertMappedIcon(GroupApiVersionKind.From<V1HorizontalPodAutoscaler>(), "/Assets/kube/resources/unlabeled/hpa.svg");
    }

    [AvaloniaFact]
    public void User_subject_icon_mapping_is_preserved()
    {
        AssertMappedIcon(new GroupApiVersionKind(string.Empty, "v1", "UserSubject", string.Empty), "/Assets/kube/resources/unlabeled/user.svg");
    }

    private static void AssertMappedIcon(GroupApiVersionKind kind, string expectedPath)
    {
        ResourceIconService service = new();

        service.GetIcon(kind).ShouldBeOfType<SvgImage>().Source!.Path.ShouldBe(expectedPath);
    }

    [AvaloniaFact]
    public void Known_resource_api_key_without_plural_returns_mapped_icon()
    {
        ResourceIconService service = new();

        var icon = service.GetIcon(new GroupApiVersionKind(string.Empty, "v1", "Pod", string.Empty));

        icon.ShouldBeOfType<SvgImage>().Source!.Path.ShouldBe("/Assets/kube/resources/unlabeled/pod.svg");
    }

    [AvaloniaFact]
    public void Unknown_resource_type_returns_generated_icon()
    {
        ResourceIconService service = new();

        var icon = service.GetIcon(new GroupApiVersionKind("monitoring.example.com", "v1", "Alertmanager", "alertmanagers"));

        var svgIcon = icon.ShouldBeOfType<SvgImage>();
        svgIcon.Source.ShouldNotBeNull();
        svgIcon.Source!.Path.ShouldBeNull();
        svgIcon.Source.Picture.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public void Repeated_resource_type_requests_reuse_parsed_svg_source()
    {
        ResourceIconService service = new();

        var kind = new GroupApiVersionKind("monitoring.example.com", "v1", "Alertmanager", "alertmanagers");
        var first = service.GetIcon(kind);
        var second = service.GetIcon(kind);

        first.ShouldNotBeSameAs(second);
        first.ShouldBeOfType<SvgImage>().Source.ShouldBeSameAs(second.ShouldBeOfType<SvgImage>().Source);
    }

}
