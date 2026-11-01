using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Model.Core;
using KubeUI.Avalonia.Features.Crossplane.MRDiffDetection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Crossplane;

public sealed class MRDiffDetectionViewTests
{
    [AvaloniaFact]
    public void Builds_provider_search_status_clear_and_yaml_context_menu_controls()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        using var monitor = new CrossplaneProviderLogMonitor(
            Mock.Of<IPodLogSessionResolver>(),
            Mock.Of<IPodLogStreamClient>(),
            NullLogger<CrossplaneProviderLogMonitor>.Instance);
        using var viewModel = new MRDiffDetectionViewModel(
            new CrossplaneDiffLogParser(),
            monitor,
            NullLogger<MRDiffDetectionViewModel>.Instance,
            services,
            Mock.Of<IFactory>());
        MRDiffDetectionView view = new()
        {
            DataContext = viewModel
        };

        var root = view.Child.ShouldBeOfType<Grid>();
        var toolbar = root.Children.OfType<Grid>().ShouldHaveSingleItem();
        toolbar.MinHeight.ShouldBe(32d);
        toolbar.Margin.Top.ShouldBe(2d);
        toolbar.Margin.Bottom.ShouldBe(2d);

        var providerSelector = toolbar.Children.OfType<ComboBox>().ShouldHaveSingleItem();
        providerSelector.VerticalAlignment.ShouldBe(global::Avalonia.Layout.VerticalAlignment.Stretch);

        var searchBox = toolbar.Children.OfType<TextBox>().ShouldHaveSingleItem();
        searchBox.VerticalAlignment.ShouldBe(global::Avalonia.Layout.VerticalAlignment.Stretch);
        searchBox.VerticalContentAlignment.ShouldBe(global::Avalonia.Layout.VerticalAlignment.Center);
        searchBox.Background.ShouldBe(global::Avalonia.Media.Brushes.Transparent);

        toolbar.Measure(new Size(1000, 40));
        toolbar.Arrange(new Rect(0, 0, 1000, 40));
        providerSelector.Bounds.Height.ShouldBe(searchBox.Bounds.Height);

        toolbar.Children.OfType<TextBlock>().ShouldHaveSingleItem();
        toolbar.Children.OfType<Button>()
            .ShouldHaveSingleItem()
            .Command
            .ShouldBe(viewModel.ClearCommand);

        var table = root.Children.OfType<DynamicTableView>().ShouldHaveSingleItem();
        var row = new CrossplaneDiffRow(new CrossplaneDiffRecord(
            "uid-1",
            "widget",
            "default",
            "example.com/v1",
            "Widget",
            "spec.value",
            "old",
            "new",
            false,
            false,
            false));
        var menuItem = table.ContextMenuItemsFactory!(new object[] { row })!
            .Cast<MenuItem>()
            .ShouldHaveSingleItem();

        menuItem.Header.ShouldBe(Assets.Resources.ResourceConfigBase_MenuItem_ViewYaml);
        menuItem.Command.ShouldBe(viewModel.ViewYamlCommand);
        menuItem.CommandParameter.ShouldBeSameAs(row);
        table.ContextMenuItemsFactory!([new object()])!.Cast<object>().ShouldBeEmpty();
    }
}
