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
public static partial class CollectionViewSource_MarkupExtensions
{
//================= Properties ======================//
 // IsSourceGroupedProperty

/*BindFromExpressionSetterGenerator*/
public static T IsSourceGrouped<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.IsSourceGroupedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsSourceGrouped<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
=> control._setEx(FluentAvalonia.UI.Data.CollectionViewSource.IsSourceGroupedProperty, ps, () => control.IsSourceGrouped = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsSourceGrouped<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.IsSourceGroupedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsSourceGrouped<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.IsSourceGroupedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsSourceGrouped<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
=> control._setEx(FluentAvalonia.UI.Data.CollectionViewSource.IsSourceGroupedProperty, ps, () => control.IsSourceGrouped = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ItemsBindingProperty

/*BindFromExpressionSetterGenerator*/
public static T ItemsBinding<T>(this T control, Func<Avalonia.Data.IBinding> func, Action<Avalonia.Data.IBinding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.ItemsBindingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ItemsBinding<T>(this T control, Avalonia.Data.IBinding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
=> control._setEx(FluentAvalonia.UI.Data.CollectionViewSource.ItemsBindingProperty, ps, () => control.ItemsBinding = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemsBinding<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.ItemsBindingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemsBinding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.ItemsBindingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ItemsBinding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.IBinding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
=> control._setEx(FluentAvalonia.UI.Data.CollectionViewSource.ItemsBindingProperty, ps, () => control.ItemsBinding = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SourceProperty

/*BindFromExpressionSetterGenerator*/
public static T Source<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.SourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Source<T>(this T control, System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
=> control._setEx(FluentAvalonia.UI.Data.CollectionViewSource.SourceProperty, ps, () => control.Source = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Source<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.SourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Source<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.SourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Source<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
=> control._setEx(FluentAvalonia.UI.Data.CollectionViewSource.SourceProperty, ps, () => control.Source = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FilterProperty

/*BindFromExpressionSetterGenerator*/
public static T Filter<T>(this T control, Func<System.Predicate<System.Object>> func, Action<System.Predicate<System.Object>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.FilterProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Filter<T>(this T control, System.Predicate<System.Object> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
=> control._setEx(FluentAvalonia.UI.Data.CollectionViewSource.FilterProperty, ps, () => control.Filter = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Filter<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.FilterProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Filter<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.FilterProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Filter<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Predicate<System.Object>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
=> control._setEx(FluentAvalonia.UI.Data.CollectionViewSource.FilterProperty, ps, () => control.Filter = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsLiveShapingEnabledProperty

/*BindFromExpressionSetterGenerator*/
public static T IsLiveShapingEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.IsLiveShapingEnabledProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsLiveShapingEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
=> control._setEx(FluentAvalonia.UI.Data.CollectionViewSource.IsLiveShapingEnabledProperty, ps, () => control.IsLiveShapingEnabled = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsLiveShapingEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.IsLiveShapingEnabledProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsLiveShapingEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
   => control._set(FluentAvalonia.UI.Data.CollectionViewSource.IsLiveShapingEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsLiveShapingEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Data.CollectionViewSource
=> control._setEx(FluentAvalonia.UI.Data.CollectionViewSource.IsLiveShapingEnabledProperty, ps, () => control.IsLiveShapingEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//

}
