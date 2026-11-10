using k8s.Models;

namespace KubeUI.Features.Resources.Storage.v1.StorageClass;

public sealed partial class V1StorageClassConfig : ResourceConfig<V1StorageClass>
{
    public V1StorageClassConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.StorageId);
    public override int Order => 2;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1StorageClass, string>()
            {
                Key = "provisioner",
                Name = ResourcesLanguageManager.Instance.V1StorageClassConfig_Provisioner.CurrentValue()!,
                Field = x => x.Provisioner,
                Width = 1, WidthMode = DynamicTableViewWidthMode.Star,
            },
            new ResourceListColumn<V1StorageClass, string>()
            {
                Key = "reclaim-policy",
                Name = ResourcesLanguageManager.Instance.V1StorageClassConfig_Reclaim_Policy.CurrentValue()!,
                Field = x => x.ReclaimPolicy,
                WidthMode = DynamicTableViewWidthMode.Cells
            },
            new ResourceListColumn<V1StorageClass, string>()
            {
                Key = "default",
                Name = ResourcesLanguageManager.Instance.V1StorageClassConfig_Default.CurrentValue()!, // "storageclass.kubernetes.io/is-default-class":"true"
                Field = x => x.Metadata.Annotations?.ContainsKey("storageclass.kubernetes.io/is-default-class") == true ?
                                x.Metadata.Annotations["storageclass.kubernetes.io/is-default-class"] : "false",
                WidthMode = DynamicTableViewWidthMode.Cells
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1StorageClass resource) => [new PropertiesView()];
}
