#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
[global::System.CodeDom.Compiler.GeneratedCode("AvaloniaExtensionGenerator", "11.1.3.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class MultiComboBoxItem_MarkupExtensions
{
//================= Properties ======================//
 // IsSelectedProperty

/*BindFromExpressionSetterGenerator*/
public static T IsSelected<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.MultiComboBoxItem
   => control._set(Ursa.Controls.MultiComboBoxItem.IsSelectedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsSelected<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBoxItem
=> control._setEx(Ursa.Controls.MultiComboBoxItem.IsSelectedProperty, ps, () => control.IsSelected = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsSelected<T>(this T control, IBinding binding) where T : Ursa.Controls.MultiComboBoxItem
   => control._set(Ursa.Controls.MultiComboBoxItem.IsSelectedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsSelected<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.MultiComboBoxItem
   => control._set(Ursa.Controls.MultiComboBoxItem.IsSelectedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsSelected<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.MultiComboBoxItem
=> control._setEx(Ursa.Controls.MultiComboBoxItem.IsSelectedProperty, ps, () => control.IsSelected = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // IsSelectedProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsSelected<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.MultiComboBoxItem
=> style._addSetter(Ursa.Controls.MultiComboBoxItem.IsSelectedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsSelected<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MultiComboBoxItem
=> style._addSetter(Ursa.Controls.MultiComboBoxItem.IsSelectedProperty, binding);



}