using Avalonia.Data.Converters;
using Avalonia.Styling;
using k8s.Models;
using KubeUI.Avalonia.Controls;

namespace KubeUI.Avalonia.Resources.Core.v1;

public sealed partial class V1EventConfig : ResourceConfigBase<Corev1Event>
{
    public override bool IsNamespaced => true;
    public override bool ShowNewResource => false;
    public override int Order => 7;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            new ResourceListColumn<Corev1Event, string>()
            {
                Name = "Type",
                Field = x => x?.Type ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListColumn<Corev1Event, string>()
            {
                Name = "Message",
                Field = x => x?.Message ?? "",
                Width = "4*"
            },
            NamespaceColumn(),
            new ResourceListColumn<Corev1Event, string>()
            {
                Name = "Involved Object",
                Field = x => x?.InvolvedObject?.Name ?? "",
                Width = "*"
            },
            new ResourceListColumn<Corev1Event, string>()
            {
                Name = "Source",
                Field = x => x?.Source?.Component ?? (x?.ReportingComponent) ?? "",
                Width = "*"
            },
            new ResourceListColumn<Corev1Event, int>()
            {
                Name = "Count",
                Field = x => x.Count ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<Corev1Event, DateTime?>()
            {
                Name = "Last Seen",
                CustomControl = typeof(EventLastSeenCell),
                Field = x => x.LastTimestamp ?? (x.EventTime ?? x.Metadata.CreationTimestamp),
                Sort = SortDirection.Descending,
                Width = "80"
            },
            AgeColumn(),
        ];
    }

    public override IStyle ListStyle()
    {
        var style = new Style(x => x.OfType<DataGridRow>());
        style.Add(new Setter(DataGridRow.ForegroundProperty, new Binding("Type")
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
            })
        }));

        return style;
    }
}

