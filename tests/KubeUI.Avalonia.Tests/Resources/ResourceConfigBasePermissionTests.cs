using Avalonia.Headless.XUnit;
using Dock.Model.Controls;
using Dock.Model.Core;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Avalonia.Resources;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Resources;

public sealed class ResourceConfigBasePermissionTests
{
    [AvaloniaFact]
    public void permissions_manifest_includes_default_and_custom_permissions()
    {
        var services = Application.Current.GetTestServices();
        var config = new TrackingResourceConfig(services);

        config.Permissions().ShouldBe(
        [
            (Verb.Create, null),
            (Verb.Delete, null),
            (Verb.List, null),
            (Verb.Patch, null),
            (Verb.Update, null),
            (Verb.Watch, null),
            (Verb.Get, "status")
        ]);
    }

    [AvaloniaFact]
    public void default_context_menu_includes_resource_editor_for_a_single_resource()
    {
        var services = Application.Current.GetTestServices();
        var config = new TrackingResourceConfig(services);
        var pod = new V1Pod { Metadata = new V1ObjectMeta { Name = "pod-1", NamespaceProperty = "default" } };

        config.GetDefaultMenuItems(new[] { pod })
            .ShouldContain(item => item.Title == "View YAML");

        var edit = config.GetDefaultMenuItems(new[] { pod })
            .Single(item => item.Title == KubeUI.Avalonia.Assets.Resources.ResourceConfigBase_MenuItem_Edit);

        edit.Command.ShouldNotBeNull();
        edit.CommandParameter.ShouldBeAssignableTo<IList<V1Pod>>().ShouldBe([pod]);
    }

    [AvaloniaFact]
    public async Task new_yaml_resource_is_opened_in_the_documents_dock()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        var documents = factory.GetDockable<IDocumentDock>("Documents").ShouldNotBeNull();
        var config = new TrackingResourceConfig(Application.Current.GetTestServices());
        config.Initialize(cluster);

        config.NewResourceCommand.Execute(null);

        var yamlEditor = documents.VisibleDockables!.OfType<ResourceYamlViewModel>().Single();
        try
        {
            documents.VisibleDockables.ShouldContain(yamlEditor);
        }
        finally
        {
            factory.RemoveDockable(yamlEditor, collapse: false);
            yamlEditor.Dispose();
        }
    }

    private sealed class TrackingResourceConfig : ResourceConfigBase<V1Pod>
    {
        public TrackingResourceConfig(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public override bool IsNamespaced => true;

        public override IList<(Verb verb, string? subResource)> CustomPermissions() =>
        [
            (Verb.Get, "status")
        ];
    }
}
