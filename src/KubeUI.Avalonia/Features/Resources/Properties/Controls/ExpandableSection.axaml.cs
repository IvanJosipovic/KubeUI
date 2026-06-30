using Avalonia.Markup.Declarative;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

public partial class ExpandableSection : Expander, IDeclarativeViewBase
{
    public ExpandableSection()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode)
        {
            Header = "Header";
            Content = new StackPanel();
            var panel = (Content as StackPanel);

            for (int i = 0; i < 5; i++)
            {
                panel.Children.Add(new PropertyItem() { Key = "Name" + i, Value = "myValue" + i });
            }

            base.IsExpanded = true;
        }
#endif
    }

    protected override Type StyleKeyOverride => typeof(Expander);
}


