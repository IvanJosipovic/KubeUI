using System.Text;
using Avalonia.Controls.Templates;
using k8s.Models;

namespace KubeUI.Avalonia.Resources.Configuration.v1.Secret;

public sealed partial class V1SecretConfig : ResourceConfigBase<k8s.Models.V1Secret>
{
    public V1SecretConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override string Category => Assets.Resources.ResourceConfig_Category_Configuration!;
    public override int Order => 1;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<k8s.Models.V1Secret, string>()
            {
                Key = "labels",
                Name = Assets.Resources.V1SecretConfig_Labels!,
                Field = x => x.Metadata?.Labels is { Count: > 0 } labels ? string.Join(", ", labels.Keys) : "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<k8s.Models.V1Secret, string>()
            {
                Key = "keys",
                Name = Assets.Resources.V1SecretConfig_Keys!,
                Field = x => x.Data is { Count: > 0 } data ? string.Join(", ", data.Keys) : "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<k8s.Models.V1Secret, string>()
            {
                Key = "type",
                Name = Assets.Resources.V1SecretConfig_Type!,
                Field = x => x.Type,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(k8s.Models.V1Secret resource) => [new PropertiesView()];
}


