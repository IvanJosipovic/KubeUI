using Avalonia.Controls.Templates;

namespace KubeUI.Avalonia.Controls.DataGridFilters;

internal sealed partial class DateFilterFlyoutView : ViewBase<DateFilterFlyoutContext>
{
    public Control? Content => Child;

    public DateFilterFlyoutView()
    {
#if DEBUG
        if (Design.IsDesignMode && DataContext == null)
        {
            var context = new DateFilterFlyoutContext(
                title: "Last_Seen",
                timeProvider: TimeProvider.System,
                apply: static () => { },
                clear: static () => { })
            {
                Amount = 6
            };

            context.SelectedUnit = context.Units.First(x => x.Unit == DateRelativeUnit.Hours);
            DataContext = context;
        }
#endif
    }

    protected override object Build(DateFilterFlyoutContext vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return DataGridFilterFlyoutViewBuilder.CreateRoot(
            DataGridFilterFlyoutViewBuilder.CreateTitle(vm),
            DataGridFilterFlyoutViewBuilder.CreateOperatorRow(vm),
            DataGridFilterFlyoutViewBuilder.CreateRow(
                Assets.Resources.DataGridFilterFlyout_Value!,
                new Grid()
                    .Col(1)
                    .Classes("filter-flyout-composite-editor")
                    .Cols("96,8,*")
                    .Children(
                        new NumericUpDown()
                            .Col(0)
                            .Classes("filter-flyout-editor")
                            .Increment(1)
                            .Minimum(1)
                            .Value(vm, x => x.Amount, BindingMode.TwoWay),
                        new ComboBox()
                            .Col(2)
                            .Classes("filter-flyout-editor")
                            .ItemsSource(vm, x => x.Units)
                            .SelectedItem(vm, x => x.SelectedUnit, BindingMode.TwoWay)
                            .ItemTemplate(new FuncDataTemplate<DateRelativeUnitChoice>((choice, _) =>
                                new TextBlock().Text(choice?.Label ?? string.Empty))))),
            DataGridFilterFlyoutViewBuilder.CreateActions(vm));
    }
}
