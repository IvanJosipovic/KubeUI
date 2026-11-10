using Avalonia.Data.Converters;
using k8s.Models;
using KubeUI.Controls.Styles;
using KubeUI.Features.Resources.Core.v1.Event.Controls;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Core.v1.Event;

public sealed class V1EventConfig(IServiceProvider serviceProvider) : ResourceConfig<Corev1Event>(serviceProvider)
{
    public override bool IsNamespaced => true;
    public override bool SeedOnConnect => true;
    public override bool ShowNewResource => false;
    public override int Order => 7;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            new ResourceListColumn<Corev1Event, string>()
            {
                Key = "type",
                Name = ResourcesLanguageManager.Instance.V1EventConfig_Type.CurrentValue()!,
                Field = x => x?.Type ?? "",
                WidthMode = DynamicTableViewWidthMode.Cells
            },
            new ResourceListColumn<Corev1Event, string>()
            {
                Key = "message",
                Name = ResourcesLanguageManager.Instance.V1EventConfig_Message.CurrentValue()!,
                Field = x => x?.Message ?? "",
                Width = 4, WidthMode = DynamicTableViewWidthMode.Star
            },
            NamespaceColumn(),
            new ResourceListColumn<Corev1Event, string>()
            {
                Key = "involved-object",
                Name = ResourcesLanguageManager.Instance.V1EventConfig_Involved_Object.CurrentValue()!,
                Field = x => x?.InvolvedObject?.Name ?? "",
                Width = 1, WidthMode = DynamicTableViewWidthMode.Star
            },
            new ResourceListColumn<Corev1Event, string>()
            {
                Key = "source",
                Name = ResourcesLanguageManager.Instance.V1EventConfig_Source.CurrentValue()!,
                Field = x => x?.Source?.Component ?? (x?.ReportingComponent) ?? "",
                Width = 1, WidthMode = DynamicTableViewWidthMode.Star
            },
            new ResourceListColumn<Corev1Event, int>()
            {
                Key = "count",
                Name = ResourcesLanguageManager.Instance.V1EventConfig_Count.CurrentValue()!,
                Field = x => x?.Count ?? 0,
                WidthMode = DynamicTableViewWidthMode.Header
            },
            new ResourceListColumn<Corev1Event, DateTime?>()
            {
                Key = "last-seen",
                Name = ResourcesLanguageManager.Instance.V1EventConfig_Last_Seen.CurrentValue()!,
                CustomControl = typeof(EventLastSeenCell),
                Field = x => KubernetesEventTimestampResolver.Resolve(x),
                Sort = SortDirection.Descending,
                Width = 80, WidthMode = DynamicTableViewWidthMode.Pixel
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(Corev1Event resource) => [new PropertiesView()];

    public override Style[] ListStyle() =>
    [
        new Style<TableViewRow>(x => x.OfType<TableViewRow>())
            .Foreground(CompiledBinding.Create<Corev1Event, object>(x => x.Type,
                converter: new FuncValueConverter<string, IBrush>(y =>
                {
                    if (string.Equals(y, "Warning", StringComparison.Ordinal))
                    {
                        return ApplicationBrushResources.GetBrush("SystemControlErrorTextForegroundBrush");
                    }

                    return ApplicationBrushResources.GetBrush("SystemBaseHighColor");
                })))
    ];
}
