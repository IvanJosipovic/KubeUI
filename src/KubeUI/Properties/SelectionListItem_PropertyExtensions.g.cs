#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using SelectionListItem = Ursa.Controls.SelectionListItem;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class SelectionListItemExtensions
{
public static T IsSelected<T>(this T control, IBinding binding) where T : Ursa.Controls.SelectionListItem
   => control._set(Ursa.Controls.SelectionListItem.IsSelectedProperty, binding);
public static T IsSelected<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.SelectionListItem
   => control._set(Ursa.Controls.SelectionListItem.IsSelectedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsSelected<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.SelectionListItem
   => control._set(Ursa.Controls.SelectionListItem.IsSelectedProperty, func, onChanged, expression);
public static T IsSelected<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.SelectionListItem
=> control._setEx(Ursa.Controls.SelectionListItem.IsSelectedProperty, ps, () => control.IsSelected = value, bindingMode, converter, bindingSource);
public static T IsSelected<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.SelectionListItem
=> control._setEx(Ursa.Controls.SelectionListItem.IsSelectedProperty, ps, () => control.IsSelected = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

