namespace KubeUI.Controls;

public partial class DropDownCheckBox : UserControl
{
    public static readonly AvaloniaProperty ItemsProperty = AvaloniaProperty.Register<DropDownCheckBox, IEnumerable>(nameof(Items));

    public DropDownCheckBox()
    {
        InitializeComponent();
    }

    public IEnumerable Items
    {
        get { return (IEnumerable)GetValue(ItemsProperty); }
        set { SetValue(ItemsProperty, value); }
    }

    //private void InitializeComponent()
    //{
    //    AvaloniaXamlLoader.Load(this);
    //    var comboBox = this.FindControl<ComboBox>("comboBox");
    //    comboBox.Bind(ComboBox.ItemsSourceProperty, new Binding("Items") { Source = this });
    //}
}
