using System.Text;
using Avalonia.Controls.Templates;
using k8s.Models;
using KubeUI.Controls;
using KubeUI.Views;

namespace KubeUI.Resources.Configuration.V1Secret;

public sealed partial class V1SecretConfig : ResourceConfigBase<k8s.Models.V1Secret>
{
    public override bool IsNamespaced => true;
    public override string Category => "Configuration";
    public override int Order => 1;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<k8s.Models.V1Secret, string>()
            {
                Name = "Labels",
                Display = x => x.Metadata?.Labels != null ? x.Metadata.Labels.Keys.Aggregate((a,b) => a + ", " + b) : "",
                Field = x => x.Metadata?.Labels?.Keys.FirstOrDefault() ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<k8s.Models.V1Secret, string>()
            {
                Name = "Keys",
                Display = x => x.Data != null ? x.Data.Keys.Aggregate((a,b) => a + ", " + b) : "",
                Field = x => x.Data?.Keys.FirstOrDefault() ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<k8s.Models.V1Secret, string>()
            {
                Name = "Type",
                Field = x => x.Type,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(k8s.Models.V1Secret resource) => [new V1SecretProperties()];
}
