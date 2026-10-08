using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Model.Core;
using KubeUI.Avalonia.Features.Crossplane.MRDiffDetection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SvcSystems.Avalonia.DynamicTableView;

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
        var toolbar = root.Children.OfType<StackPanel>().ShouldHaveSingleItem();
        toolbar.Children.OfType<ComboBox>().ShouldHaveSingleItem();
        toolbar.Children.OfType<TextBox>().ShouldHaveSingleItem();
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
