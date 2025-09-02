using k8s.Models;
using KubeUI.Views;

namespace KubeUI.Controls;

public sealed class PodStatusCell : MyViewBase<V1Pod>
{
    protected override void OnDataContextChanged(EventArgs e)
    {
        if (ViewModel != null)
        {
            if (ViewModel.Metadata.DeletionTimestamp.HasValue)
            {
                PrettyString = "Terminating";
            }
            else
            {
                PrettyString = ViewModel.Status?.Conditions?.FirstOrDefault(x => x.Type == "Ready")?.Status == "True" ? "Running" : ViewModel.Status?.Conditions?.FirstOrDefault(x => x.Type == "Ready")?.Reason ?? "Unknown";
            }

            if (PrettyString == "Pending" || PrettyString == "Terminating")
            {
                _styles = [
                    new Style<TextBlock>()
                        .Setter(TextBlock.ForegroundProperty, Brushes.Yellow),
                    ];
            }
            else if (PrettyString == "Running" || PrettyString == "PodCompleted")
            {
                _styles = [
                    new Style<TextBlock>()
                        .Setter(TextBlock.ForegroundProperty, Brushes.LimeGreen),
                    ];
            }
            else
            {
                _styles = [
                    new Style<TextBlock>()
                        .Setter(TextBlock.ForegroundProperty, Brushes.Orange),
                    ];
            }
        }

        base.OnDataContextChanged(e);
    }

    public static readonly DirectProperty<PodStatusCell, string> PrettyStringProperty =
        AvaloniaProperty.RegisterDirect<PodStatusCell, string>(
        nameof(PrettyString),
        o => o.PrettyString,
        (o, v) => o.PrettyString = v);

    public string PrettyString
    {
        get { return _prettyString; }
        set { SetAndRaise(PrettyStringProperty, ref _prettyString, value); }
    }

    private string _prettyString = string.Empty;

    private StyleGroup _styles = [];

    protected override StyleGroup? BuildStyles() => _styles;

    protected override object Build(V1Pod? vm) =>
        new TextBlock()
            .Margin(12, 0, 12, 0)
            .HorizontalAlignment(HorizontalAlignment.Left)
            .VerticalAlignment(VerticalAlignment.Center)
            .Text(PrettyStringProperty)
            .ToolTip(new Binding(nameof(PrettyString)) { Source = this });
}
