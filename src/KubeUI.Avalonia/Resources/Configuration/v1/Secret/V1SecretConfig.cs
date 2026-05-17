using System.Text;
using Avalonia.Controls.Templates;
using k8s.Models;
using KubeUI.Avalonia.Resources.Configuration.v1.Secret.Views;

namespace KubeUI.Avalonia.Resources.Configuration.v1.Secret;

public sealed partial class V1SecretConfig : ResourceConfigBase<k8s.Models.V1Secret>
{
    public V1SecretConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override string Category => CategoryString("ResourceConfig_Category_Configuration", "Configuration");
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
                Display = x => x.Metadata?.Labels != null ? x.Metadata.Labels.Keys.Aggregate((a,b) => a + ", " + b) : "",
                Field = x => x.Metadata?.Labels?.Keys.FirstOrDefault() ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<k8s.Models.V1Secret, string>()
            {
                Key = "keys",
                Name = Assets.Resources.V1SecretConfig_Keys!,
                Display = x => x.Data != null ? x.Data.Keys.Aggregate((a,b) => a + ", " + b) : "",
                Field = x => x.Data?.Keys.FirstOrDefault() ?? "",
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




