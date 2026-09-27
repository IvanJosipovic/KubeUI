using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Avalonia.Tests.Infra;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Services.Settings;

public sealed class AppearanceSettingsTests
{
    [AvaloniaFact]
    public void list_row_height_setting_updates_table_row_and_header_resources()
    {
        var settings = Application.Current.GetTestServices().GetRequiredService<ISettingsService>();
        var resources = Application.Current.Resources;
        var originalHeight = settings.Appearance.ListRowHeight;
        var hadRowHeight = resources.ContainsKey("DataGridRowHeight");
        var oldRowHeight = hadRowHeight ? resources["DataGridRowHeight"] : null;
        var hadHeaderHeight = resources.ContainsKey("DataGridColumnHeaderMinHeight");
        var oldHeaderHeight = hadHeaderHeight ? resources["DataGridColumnHeaderMinHeight"] : null;

        try
        {
            settings.Appearance.ListRowHeight = 30;
            settings.ApplySettings();

            resources["DataGridRowHeight"].ShouldBe(30d);
            resources["DataGridColumnHeaderMinHeight"].ShouldBe(34d);
            TableView table = new()
            {
                FontSize = 13,
                ItemsSource = new[] { "example" }
            };
            table.Columns.Add(new TableViewColumn
            {
                Header = "Name",
                CellTemplate = new FuncDataTemplate<string>((value, _) => new TextBlock { Text = value })
            });
            using var window = Application.Current.CreateTestWindow(400, 200, table);
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var row = table.GetVisualDescendants().OfType<TableViewRow>().Single();
            row.Bounds.Height.ShouldBe(30d);
        }
        finally
        {
            settings.Appearance.ListRowHeight = originalHeight;
            settings.ApplySettings();
            RestoreResource("DataGridRowHeight", hadRowHeight, oldRowHeight);
            RestoreResource("DataGridColumnHeaderMinHeight", hadHeaderHeight, oldHeaderHeight);
        }

        void RestoreResource(string key, bool existed, object? value)
        {
            if (existed)
                resources[key] = value!;
            else
                resources.Remove(key);
        }
    }
}
