using Avalonia.Controls.Templates;

namespace KubeUI.Avalonia.Controls.DataGridFilters;

internal sealed partial class EnumFilterFlyoutView : ViewBase<EnumFilterFlyoutContext>
{
    public Control? Content => Child;

    public EnumFilterFlyoutView()
    {
#if DEBUG
        if (Design.IsDesignMode && DataContext == null)
        {
            var context = new EnumFilterFlyoutContext(
                title: "Status",
                enumType: typeof(DesignerFilterStatus),
                apply: static () => { },
                clear: static () => { });

            context.SelectedValue = context.Options.ElementAtOrDefault(1) ?? context.Options.FirstOrDefault();
            DataContext = context;
        }
#endif
    }

    protected override object Build(EnumFilterFlyoutContext vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return DataGridFilterFlyoutViewBuilder.CreateRoot(
            DataGridFilterFlyoutViewBuilder.CreateTitle(vm),
            DataGridFilterFlyoutViewBuilder.CreateOperatorRow(vm),
            DataGridFilterFlyoutViewBuilder.CreateRow(
                Assets.Resources.DataGridFilterFlyout_Value,
                new ComboBox()
                    .Col(1)
                    .Classes("filter-flyout-editor")
                    .ItemsSource(vm, x => x.Options)
                    .SelectedItem(vm, x => x.SelectedValue, BindingMode.TwoWay)
                    .ItemTemplate(new FuncDataTemplate<EnumFilterChoice>((choice, _) =>
                        new TextBlock().Text(choice?.Label ?? string.Empty)))),
            DataGridFilterFlyoutViewBuilder.CreateActions(vm));
    }

#if DEBUG
    private enum DesignerFilterStatus
    {
        Pending,
        Running,
        Failed
    }
#endif
}
