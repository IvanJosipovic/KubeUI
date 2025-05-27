#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
[global::System.CodeDom.Compiler.GeneratedCode("AvaloniaExtensionGenerator", "1.0.0.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class NavigationViewItemPresenter_MarkupExtensions
{
//================= Properties ======================//
 // IconSource

/*BindFromExpressionSetterGenerator*/
public static T IconSource<T>(this T control, Func<FluentAvalonia.UI.Controls.IconSource> func, Action<FluentAvalonia.UI.Controls.IconSource>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter 
   => control._set(FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter.IconSourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IconSource<T>(this T control,FluentAvalonia.UI.Controls.IconSource value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter 
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter.IconSourceProperty, ps, () => control.IconSource = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IconSource<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter 
   => control._set(FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter.IconSourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IconSource<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter 
   => control._set(FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter.IconSourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IconSource<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.IconSource> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter 
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter.IconSourceProperty, ps, () => control.IconSource = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InfoBadge

/*BindFromExpressionSetterGenerator*/
public static T InfoBadge<T>(this T control, Func<FluentAvalonia.UI.Controls.InfoBadge> func, Action<FluentAvalonia.UI.Controls.InfoBadge>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter 
   => control._set(FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter.InfoBadgeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InfoBadge<T>(this T control,FluentAvalonia.UI.Controls.InfoBadge value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter 
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter.InfoBadgeProperty, ps, () => control.InfoBadge = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InfoBadge<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter 
   => control._set(FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter.InfoBadgeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InfoBadge<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter 
   => control._set(FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter.InfoBadgeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InfoBadge<TValue,T>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.InfoBadge> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter 
=> control._setEx(FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter.InfoBadgeProperty, ps, () => control.InfoBadge = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Styles ======================//
 // IconSource

/*ValueStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter 
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter.IconSourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter 
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter.IconSourceProperty, binding);


 // InfoBadge

/*ValueStyleSetterGenerator*/
public static Style<T> InfoBadge<T>(this Style<T> style, FluentAvalonia.UI.Controls.InfoBadge value) where T : FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter 
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter.InfoBadgeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> InfoBadge<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter 
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.NavigationViewItemPresenter.InfoBadgeProperty, binding);



}
