namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

public partial class ExpandableSection : Expander, IDeclarativeViewBase
{
    public ExpandableSection()
    {
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Top;
        IsExpanded = true;
        Resources.Add("ExpanderMinHeight", 28d);
        Resources.Add("ExpanderHeaderPadding", new Thickness(10, 0, 0, 0));
        Resources.Add("ExpanderChevronMargin", new Thickness(8, 0, 4, 0));
        Resources.Add("ExpanderChevronButtonSize", 24d);
        Resources.Add("ExpanderContentPadding", new Thickness(5, 0, 0, 0));

#if DEBUG
        if (Design.IsDesignMode)
        {
            Header = "Header";
            Content = new StackPanel();
            var panel = (Content as StackPanel);

            for (var i = 0; i < 5; i++)
            {
                panel.Children.Add(new PropertyItem() { Key = "Name" + i, Value = "myValue" + i });
            }

            base.IsExpanded = true;
        }
#endif
    }

    protected override Type StyleKeyOverride => typeof(Expander);
}


