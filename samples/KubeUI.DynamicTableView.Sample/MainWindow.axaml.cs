using Avalonia.Controls;
using System.Collections.ObjectModel;

namespace KubeUI.DynamicTableView.Sample;

public sealed partial class MainWindow : Window, IDisposable
{
    private readonly DynamicTableViewSource<SampleRow, string> _xamlSource;
    private readonly DynamicTableViewSource<SampleRow, string> _codeSource;
    private bool _disposed;

    public MainWindow()
    {
        InitializeComponent();
        var xamlColumns = (SampleColumnDefinitions)Resources["XamlColumnDefinitions"]!;
        var rows = new ObservableCollection<SampleRow>(SampleRows.Create());
        _xamlSource = DynamicTableViewSource<SampleRow, string>.FromObservableCollection(
            rows, static row => row.Id, xamlColumns.CreateColumns());
        _codeSource = DynamicTableViewSource<SampleRow, string>.FromObservableCollection(
            rows, static row => row.Id, CreateCodeColumns());

        XamlTable.Source = _xamlSource;
        CodeFirstHost.Content = new DynamicTableView { Source = _codeSource };
        Closing += (_, _) => Dispose();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _xamlSource.Dispose();
        _codeSource.Dispose();
    }

    private static DynamicTableViewColumn<SampleRow>[] CreateCodeColumns()
    {
        var name = DynamicTableViewColumn<SampleRow>.Create("name", "Name", static row => row.Name);
        name.WidthMode = DynamicTableViewWidthMode.Star;
        return
        [
            name,
            DynamicTableViewColumn<SampleRow>.Create("restarts", "Restarts", static row => row.Restarts),
            DynamicTableViewColumn<SampleRow>.Create("ready", "Ready", static row => row.Ready),
            DynamicTableViewColumn<SampleRow>.Create("cpuCores", "CPU cores", static row => row.CpuCores),
            DynamicTableViewColumn<SampleRow>.Create("memory", "Memory", static row => row.Memory),
            DynamicTableViewColumn<SampleRow>.Create("createdAt", "Created", static row => row.CreatedAt)
        ];
    }
}
