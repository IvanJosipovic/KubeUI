using Avalonia.Controls.Templates;

namespace KubeUI.Avalonia.Controls.DataGridFilters;

internal sealed partial class TextFilterFlyoutView : ViewBase<TextFilterFlyoutContext>
{
    public Control? Content => Child;

    public TextFilterFlyoutView()
    {
#if DEBUG
        if (Design.IsDesignMode && DataContext == null)
        {
            DataContext = new TextFilterFlyoutContext(
                title: "Name",
                apply: static () => { },
                clear: static () => { })
            {
                Query = "nginx"
            };
        }
#endif
    }

    protected override object Build(TextFilterFlyoutContext vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return DataGridFilterFlyoutViewBuilder.CreateRoot(
            DataGridFilterFlyoutViewBuilder.CreateTitle(vm),
            DataGridFilterFlyoutViewBuilder.CreateOperatorRow(vm),
            DataGridFilterFlyoutViewBuilder.CreateRow(
                Assets.Resources.DataGridFilterFlyout_Value,
                new TextBox()
                    .Col(1)
                    .Classes("filter-flyout-editor")
                    .Text(vm, x => x.Query, BindingMode.TwoWay)),
            DataGridFilterFlyoutViewBuilder.CreateActions(vm));
    }
}

internal static class DataGridFilterFlyoutViewBuilder
{
    public static StackPanel CreateRoot(params Control[] children)
    {
        return new StackPanel()
            .Classes("filter-flyout-root")
            .Children(children);
    }

    public static TextBlock CreateTitle<TContext>(TContext vm)
        where TContext : ColumnFilterFlyoutContextBase
    {
        return new TextBlock()
            .Classes("filter-flyout-title")
            .Text(vm, x => x.Title);
    }

    public static Grid CreateOperatorRow<TContext>(TContext vm)
        where TContext : ColumnFilterFlyoutContextBase
    {
        return CreateRow(
            Assets.Resources.DataGridFilterFlyout_Condition,
            new ComboBox()
                .Col(1)
                .Classes("filter-flyout-editor")
                .ItemsSource(vm, x => x.Operators)
                .SelectedItem(vm, x => x.SelectedOperator, BindingMode.TwoWay)
                .ItemTemplate(new FuncDataTemplate<FilterOperatorChoice>((choice, _) =>
                    new TextBlock().Text(choice?.Label ?? string.Empty))));
    }

    public static Grid CreateRow(string label, Control editor)
    {
        return new Grid()
            .Classes("filter-flyout-row")
            .Cols("72,*")
            .Children(
                new TextBlock()
                    .Classes("filter-flyout-label")
                    .Text(label),
                editor);
    }

    public static StackPanel CreateActions<TContext>(TContext vm)
        where TContext : ColumnFilterFlyoutContextBase
    {
        return new StackPanel()
            .Classes("filter-flyout-actions")
            .Children(
                new Button()
                    .Classes("filter-flyout-action")
                    .Command(vm, x => x.ClearCommand)
                    .Content(Assets.Resources.DataGridFilterFlyout_Clear),
                new Button()
                    .Classes("filter-flyout-action")
                    .Command(vm, x => x.ApplyCommand)
                    .Content(Assets.Resources.DataGridFilterFlyout_Apply));
    }
}
