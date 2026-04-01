using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using KubeUI.Avalonia.Resources;

namespace KubeUI.Avalonia.Controls.DataGridFilters;

internal sealed class DataGridColumnFilterFlyoutFactory
{
    private readonly DataGridColumnFilterService _filterService;

    public DataGridColumnFilterFlyoutFactory(DataGridColumnFilterService filterService)
    {
        _filterService = filterService;
    }

    public FlyoutBase Create(IResourceListColumn columnDefinition, DataGridColumnDefinition column, IFilteringModel filteringModel)
    {
        var definition = _filterService.CreateDefinition(columnDefinition, column, filteringModel);
        var flyout = new Flyout
        {
            Content = CreateContent(definition)
        };

        flyout.Opened += (_, _) => definition.Load();

        if (Application.Current?.TryFindResource("DataGridFilterFlyoutPresenterTheme", out var presenterTheme) == true &&
            presenterTheme is ControlTheme controlTheme)
        {
            flyout.FlyoutPresenterTheme = controlTheme;
        }

        return flyout;
    }

    private static Control CreateContent(DataGridColumnFilterDefinition definition)
    {
        Control control = definition.Kind switch
        {
            DataGridColumnFilterKind.Enum => new EnumFilterFlyoutView(),
            DataGridColumnFilterKind.Numeric => new NumericFilterFlyoutView(),
            DataGridColumnFilterKind.Date => new DateFilterFlyoutView(),
            _ => new TextFilterFlyoutView(),
        };

        control.DataContext = definition.Context;
        return control;
    }
}
