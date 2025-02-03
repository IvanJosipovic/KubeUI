using Avalonia.Data.Converters;
using Avalonia.Styling;
using k8s.Models;
using KubeUI.Controls;
using Scrutor;

namespace KubeUI.Resources.Workloads.Pod;

[ServiceDescriptor<ResourceConfigBase<Corev1Event>>(ServiceLifetime.Transient)]
public sealed partial class V1EventConfig : ResourceConfigBase<Corev1Event>
{
    public new bool ShowNewResource => false;
    public override int Order => 7;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            new ResourceListViewDefinitionColumn<Corev1Event, string>()
            {
                Name = "Type",
                Field = x => x?.Type ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListViewDefinitionColumn<Corev1Event, string>()
            {
                Name = "Message",
                Field = x => x?.Message ?? "",
                Width = "4*"
            },
            NamespaceColumn(),
            new ResourceListViewDefinitionColumn<Corev1Event, string>()
            {
                Name = "Involved Object",
                Field = x => x?.InvolvedObject?.Name ?? "",
                Width = "*"
            },
            new ResourceListViewDefinitionColumn<Corev1Event, string>()
            {
                Name = "Source",
                Field = x => x?.Source?.Component ?? "",
                Width = "*"
            },
            new ResourceListViewDefinitionColumn<Corev1Event, int>()
            {
                Name = "Count",
                Display = x => (x.Count ?? 0).ToString(),
                Field = x => x.Count ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListViewDefinitionColumn<Corev1Event, DateTime?>()
            {
                Name = "Last Seen",
                CustomControl = typeof(LastSeenCell),
                Field = x => x.LastTimestamp,
                Display = x => x.LastTimestamp?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                Sort = SortDirection.Descending,
                Width = "80"
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [

        ];
    }

    public override Control[]? Properties(Corev1Event resource)
    {
        return null;
    }

    public new Func<StyleGroup>? SetStyle = () => [
        new Style<DataGridRow>()
            .Setter(DataGridRow.ForegroundProperty, new Binding("Value.Type")
            {
                Converter = new FuncValueConverter<string, IBrush>(x =>
                {
                    if (string.Equals(x, "Warning", StringComparison.Ordinal))
                    {
                        return Brushes.Red;
                    }

                    if (Application.Current.ActualThemeVariant == ThemeVariant.Light)
                    {
                        return Brushes.Black; //todo reference style
                    }

                    return Brushes.White; //todo reference style
                }),
            }),
    ];
}
