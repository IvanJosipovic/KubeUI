using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using System.Reactive.Concurrency;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SvcSystems.Avalonia.DynamicTableView;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Styles;

public sealed class DynamicTableViewBackgroundStyleTests
{
    [AvaloniaFact]
    public void table_rows_have_no_bottom_padding()
    {
        var table = new DynamicTableView
        {
            ItemsSource = new[] { "row" }
        };
        table.Columns.Add(new TableViewColumn
        {
            Header = "Name",
            CellTemplate = new FuncDataTemplate<string>((value, _) => new TextBlock { Text = value })
        });
        Window window = new() { Width = 320, Height = 160, Content = table };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            table.GetVisualDescendants().OfType<TableViewRow>().Single().Padding.Bottom.ShouldBe(0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void table_cell_content_is_vertically_centered()
    {
        var table = new DynamicTableView
        {
            ItemsSource = new[] { "row" }
        };
        table.Columns.Add(new TableViewColumn
        {
            Header = "Name",
            CellTemplate = new FuncDataTemplate<string>((value, _) => new TextBlock { Text = value })
        });
        Window window = new() { Width = 320, Height = 160, Content = table };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            table.GetVisualDescendants().OfType<TableViewCell>().Single()
                .VerticalContentAlignment.ShouldBe(VerticalAlignment.Center);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void table_headers_use_alt_high_background()
    {
        var application = Application.Current!;
        var originalVariant = application.RequestedThemeVariant;
        var table = new DynamicTableView
        {
            ItemsSource = new[] { "row" }
        };
        table.Columns.Add(new TableViewColumn
        {
            Header = "Name",
            CellTemplate = new FuncDataTemplate<string>((value, _) => new TextBlock { Text = value })
        });
        Window window = new() { Width = 320, Height = 160, Content = table };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var headers = table.GetVisualDescendants().OfType<TableViewColumnHeader>().ToArray();
            headers.ShouldNotBeEmpty();

            foreach (var variant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
            {
                application.RequestedThemeVariant = variant;
                Dispatcher.UIThread.RunJobs();

                application.TryGetResource("SystemAltHighColor", variant, out var altHighColorValue).ShouldBeTrue();
                var altHighColor = altHighColorValue.ShouldBeOfType<Color>();
                foreach (var header in headers)
                {
                    header.Background.ShouldBeAssignableTo<ISolidColorBrush>().Color.ShouldBe(altHighColor);
                    header.GetVisualDescendants()
                        .OfType<ContentPresenter>()
                        .Single(presenter => ReferenceEquals(presenter.Content, header.Content))
                        .Background.ShouldBeAssignableTo<ISolidColorBrush>()
                        .Color.ShouldBe(altHighColor);
                }
            }
        }
        finally
        {
            application.RequestedThemeVariant = originalVariant;
            Dispatcher.UIThread.RunJobs();
            window.Close();
        }
    }

    [AvaloniaFact]
    public void filter_button_remains_transparent_over_themed_header()
    {
        var application = Application.Current!;
        var originalVariant = application.RequestedThemeVariant;
        using var source = DynamicTableViewSource<FilterTestRow, string>.FromObservableCollection(
            new ObservableCollection<FilterTestRow> { new("row", "Name") },
            static row => row.Id,
            [DynamicTableViewColumn<FilterTestRow>.Create("name", "Name", static row => row.Name)],
            ImmediateScheduler.Instance,
            ImmediateScheduler.Instance);
        var table = new DynamicTableView { Source = source };
        Window window = new() { Width = 320, Height = 160, Content = table };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var header = table.GetVisualDescendants().OfType<TableViewColumnHeader>().Single();
            var filterButton = header.GetVisualDescendants()
                .OfType<Button>()
                .Single(button => button.Classes.Contains("dynamic-table-view-filter-button"));
            foreach (var variant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
            {
                application.RequestedThemeVariant = variant;
                Dispatcher.UIThread.RunJobs();

                filterButton.Background.ShouldBe(Brushes.Transparent);
                header.Background.ShouldBeAssignableTo<ISolidColorBrush>();
            }
        }
        finally
        {
            application.RequestedThemeVariant = originalVariant;
            Dispatcher.UIThread.RunJobs();
            window.Close();
        }
    }

    private sealed record FilterTestRow(string Id, string Name);
}
