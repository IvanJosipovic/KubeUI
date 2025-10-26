using Avalonia;
using Avalonia.Controls;
using System;

namespace KubeUI.Controls;

public partial class PropertyItem : UserControl
{
    public static readonly DirectProperty<PropertyItem, string> KeyProperty =
        AvaloniaProperty.RegisterDirect<PropertyItem, string>(
            nameof(Key),
            o => o.Key,
            (o, v) => o.Key = v);

    private string _key = string.Empty;
    public string Key
    {
        get => _key;
        set => SetAndRaise(KeyProperty, ref _key, value);
    }

    public static readonly DirectProperty<PropertyItem, string> ValueProperty =
        AvaloniaProperty.RegisterDirect<PropertyItem, string>(
            nameof(Value),
            o => o.Value,
            (o, v) => o.Value = v);

    private string _value = string.Empty;
    public string Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }

    public PropertyItem()
    {
        InitializeComponent();
        DataContext = this;
    }
}

public static class PropertyItemExtensions
{
    public static PropertyItem Key(this PropertyItem prop, string key)
    {
        prop.Key = key;
        return prop;
    }

    public static PropertyItem Value(this PropertyItem prop, string value)
    {
        prop.Value = value;
        return prop;
    }
}
