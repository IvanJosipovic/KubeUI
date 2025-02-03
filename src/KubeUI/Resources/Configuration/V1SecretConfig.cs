using System.Text;
using Avalonia.Controls.Templates;
using k8s.Models;
using KubeUI.Views;
using Scrutor;

namespace KubeUI.Resources.Workloads.Pod;

[ServiceDescriptor<ResourceConfigBase<V1Secret>>(ServiceLifetime.Transient)]
public sealed partial class V1SecretConfig : ResourceConfigBase<V1Secret>
{
    public override string Category => "Configuration";
    public override int Order => 1;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListViewDefinitionColumn<V1Secret, string>()
            {
                Name = "Labels",
                Display = x => x.Metadata?.Labels != null ? x.Metadata.Labels.Keys.Aggregate((a,b) => a + ", " + b) : "",
                Field = x => x.Metadata?.Labels?.Keys.FirstOrDefault() ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListViewDefinitionColumn<V1Secret, string>()
            {
                Name = "Keys",
                Display = x => x.Data != null ? x.Data.Keys.Aggregate((a,b) => a + ", " + b) : "",
                Field = x => x.Data?.Keys.FirstOrDefault() ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListViewDefinitionColumn<V1Secret, string>()
            {
                Name = "Type",
                Field = x => x.Type,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [

        ];
    }

    public override Control[] Properties(V1Secret resource)
    {
        return [
                new ExpandableSection()
                    .Text("Data")
                    .IsExpanded(true)
                    .Controls([
                        new ItemsControl()
                            .ItemsSource(resource.Data)
                            .ItemTemplate(new FuncDataTemplate<KeyValuePair<string, byte[]>>((x,_) =>
                                new PropertyItem()
                                    .Key(@x.Key)
                                    .Value(Encoding.UTF8.GetString(x.Value))
                            ))
                    ]),
                new ExpandableSection()
                    .Text("Certificates")
                    .IsExpanded(true)
                    .Controls([
                        new ItemsControl()
                            .ItemsSource(resource.Data)
                            .ItemTemplate(new FuncDataTemplate<KeyValuePair<string, byte[]>>((x,_) =>
                                new CertificateItemView()
                                    .Header(@x.Key)
                                    .Bytes(@x.Value)
                            ))
                    ]),
            ];
    }
}
