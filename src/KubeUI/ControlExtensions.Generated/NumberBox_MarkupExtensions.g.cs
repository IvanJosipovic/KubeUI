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
public static partial class NumberBox_MarkupExtensions
{
//================= Properties ======================//
 // AcceptsExpressionProperty

/*BindFromExpressionSetterGenerator*/
public static T AcceptsExpression<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.AcceptsExpressionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AcceptsExpression<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.AcceptsExpressionProperty, ps, () => control.AcceptsExpression = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AcceptsExpression<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.AcceptsExpressionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AcceptsExpression<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.AcceptsExpressionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AcceptsExpression<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.AcceptsExpressionProperty, ps, () => control.AcceptsExpression = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DescriptionProperty

/*BindFromExpressionSetterGenerator*/
public static T Description<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.DescriptionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Description<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.DescriptionProperty, ps, () => control.Description = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Description<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.DescriptionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Description<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.DescriptionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Description<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.DescriptionProperty, ps, () => control.Description = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HeaderProperty

/*BindFromExpressionSetterGenerator*/
public static T Header<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.HeaderProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Header<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.HeaderProperty, ps, () => control.Header = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Header<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.HeaderProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Header<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.HeaderProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Header<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.HeaderProperty, ps, () => control.Header = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HeaderTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T HeaderTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.HeaderTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HeaderTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.HeaderTemplateProperty, ps, () => control.HeaderTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HeaderTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.HeaderTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HeaderTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.HeaderTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HeaderTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.HeaderTemplateProperty, ps, () => control.HeaderTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsWrapEnabledProperty

/*BindFromExpressionSetterGenerator*/
public static T IsWrapEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.IsWrapEnabledProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsWrapEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.IsWrapEnabledProperty, ps, () => control.IsWrapEnabled = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsWrapEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.IsWrapEnabledProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsWrapEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.IsWrapEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsWrapEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.IsWrapEnabledProperty, ps, () => control.IsWrapEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // LargeChangeProperty

/*BindFromExpressionSetterGenerator*/
public static T LargeChange<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.LargeChangeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T LargeChange<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.LargeChangeProperty, ps, () => control.LargeChange = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T LargeChange<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.LargeChangeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T LargeChange<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.LargeChangeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T LargeChange<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.LargeChangeProperty, ps, () => control.LargeChange = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MinimumProperty

/*BindFromExpressionSetterGenerator*/
public static T Minimum<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.MinimumProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Minimum<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.MinimumProperty, ps, () => control.Minimum = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Minimum<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.MinimumProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Minimum<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.MinimumProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Minimum<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.MinimumProperty, ps, () => control.Minimum = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MaximumProperty

/*BindFromExpressionSetterGenerator*/
public static T Maximum<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.MaximumProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Maximum<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.MaximumProperty, ps, () => control.Maximum = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Maximum<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.MaximumProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Maximum<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.MaximumProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Maximum<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.MaximumProperty, ps, () => control.Maximum = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PlaceholderTextProperty

/*BindFromExpressionSetterGenerator*/
public static T PlaceholderText<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.PlaceholderTextProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PlaceholderText<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.PlaceholderTextProperty, ps, () => control.PlaceholderText = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PlaceholderText<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.PlaceholderTextProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PlaceholderText<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.PlaceholderTextProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PlaceholderText<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.PlaceholderTextProperty, ps, () => control.PlaceholderText = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectionFlyoutProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectionFlyout<T>(this T control, Func<Avalonia.Controls.Primitives.FlyoutBase> func, Action<Avalonia.Controls.Primitives.FlyoutBase>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SelectionFlyoutProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectionFlyout<T>(this T control, Avalonia.Controls.Primitives.FlyoutBase value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.SelectionFlyoutProperty, ps, () => control.SelectionFlyout = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectionFlyout<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SelectionFlyoutProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectionFlyout<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SelectionFlyoutProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectionFlyout<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Primitives.FlyoutBase> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.SelectionFlyoutProperty, ps, () => control.SelectionFlyout = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectionHighlightColorProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectionHighlightColor<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SelectionHighlightColorProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectionHighlightColor<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.SelectionHighlightColorProperty, ps, () => control.SelectionHighlightColor = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectionHighlightColor<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SelectionHighlightColorProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectionHighlightColor<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SelectionHighlightColorProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectionHighlightColor<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.SelectionHighlightColorProperty, ps, () => control.SelectionHighlightColor = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SmallChangeProperty

/*BindFromExpressionSetterGenerator*/
public static T SmallChange<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SmallChangeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SmallChange<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.SmallChangeProperty, ps, () => control.SmallChange = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SmallChange<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SmallChangeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SmallChange<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SmallChangeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SmallChange<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.SmallChangeProperty, ps, () => control.SmallChange = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SpinButtonPlacementModeProperty

/*BindFromExpressionSetterGenerator*/
public static T SpinButtonPlacementMode<T>(this T control, Func<FluentAvalonia.UI.Controls.NumberBoxSpinButtonPlacementMode> func, Action<FluentAvalonia.UI.Controls.NumberBoxSpinButtonPlacementMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SpinButtonPlacementModeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SpinButtonPlacementMode<T>(this T control, FluentAvalonia.UI.Controls.NumberBoxSpinButtonPlacementMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.SpinButtonPlacementModeProperty, ps, () => control.SpinButtonPlacementMode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SpinButtonPlacementMode<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SpinButtonPlacementModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SpinButtonPlacementMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SpinButtonPlacementModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SpinButtonPlacementMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.NumberBoxSpinButtonPlacementMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.SpinButtonPlacementModeProperty, ps, () => control.SpinButtonPlacementMode = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TextProperty

/*BindFromExpressionSetterGenerator*/
public static T Text<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.TextProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Text<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.TextProperty, ps, () => control.Text = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Text<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.TextProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Text<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.TextProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Text<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.TextProperty, ps, () => control.Text = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TextReadingOrderProperty

/*BindFromExpressionSetterGenerator*/
public static T TextReadingOrder<T>(this T control, Func<FluentAvalonia.UI.Controls.TextReadingOrder> func, Action<FluentAvalonia.UI.Controls.TextReadingOrder>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.TextReadingOrderProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TextReadingOrder<T>(this T control, FluentAvalonia.UI.Controls.TextReadingOrder value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.TextReadingOrderProperty, ps, () => control.TextReadingOrder = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TextReadingOrder<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.TextReadingOrderProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TextReadingOrder<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.TextReadingOrderProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TextReadingOrder<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.TextReadingOrder> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.TextReadingOrderProperty, ps, () => control.TextReadingOrder = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ValidationModeProperty

/*BindFromExpressionSetterGenerator*/
public static T ValidationMode<T>(this T control, Func<FluentAvalonia.UI.Controls.NumberBoxValidationMode> func, Action<FluentAvalonia.UI.Controls.NumberBoxValidationMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.ValidationModeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ValidationMode<T>(this T control, FluentAvalonia.UI.Controls.NumberBoxValidationMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.ValidationModeProperty, ps, () => control.ValidationMode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ValidationMode<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.ValidationModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ValidationMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.ValidationModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ValidationMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.NumberBoxValidationMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.ValidationModeProperty, ps, () => control.ValidationMode = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ValueProperty

/*BindFromExpressionSetterGenerator*/
public static T Value<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.ValueProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Value<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.ValueProperty, ps, () => control.Value = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Value<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.ValueProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Value<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.ValueProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Value<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.ValueProperty, ps, () => control.Value = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TextAlignmentProperty

/*BindFromExpressionSetterGenerator*/
public static T TextAlignment<T>(this T control, Func<Avalonia.Media.TextAlignment> func, Action<Avalonia.Media.TextAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.TextAlignmentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TextAlignment<T>(this T control, Avalonia.Media.TextAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.TextAlignmentProperty, ps, () => control.TextAlignment = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TextAlignment<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.TextAlignmentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TextAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.TextAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TextAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.TextAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.TextAlignmentProperty, ps, () => control.TextAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SimpleNumberFormatProperty

/*BindFromExpressionSetterGenerator*/
public static T SimpleNumberFormat<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SimpleNumberFormatProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SimpleNumberFormat<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.SimpleNumberFormatProperty, ps, () => control.SimpleNumberFormat = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SimpleNumberFormat<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SimpleNumberFormatProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SimpleNumberFormat<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SimpleNumberFormatProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SimpleNumberFormat<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.SimpleNumberFormatProperty, ps, () => control.SimpleNumberFormat = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // ValueChanged

/*ActionToEventGenerator*/
    public static T OnValueChanged<T>(this T control, Action<FluentAvalonia.UI.Controls.NumberBox, FluentAvalonia.UI.Controls.NumberBoxValueChangedEventArgs> action) where T : FluentAvalonia.UI.Controls.NumberBox => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.NumberBox,FluentAvalonia.UI.Controls.NumberBoxValueChangedEventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ValueChanged += h);



//================= Styles ======================//
 // AcceptsExpressionProperty

/*ValueStyleSetterGenerator*/
public static Style<T> AcceptsExpression<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.AcceptsExpressionProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AcceptsExpression<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.AcceptsExpressionProperty, binding);


 // DescriptionProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Description<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.DescriptionProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Description<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.DescriptionProperty, binding);


 // HeaderProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Header<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.HeaderProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Header<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.HeaderProperty, binding);


 // HeaderTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HeaderTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.HeaderTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HeaderTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.HeaderTemplateProperty, binding);


 // IsWrapEnabledProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsWrapEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.IsWrapEnabledProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsWrapEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.IsWrapEnabledProperty, binding);


 // LargeChangeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> LargeChange<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.LargeChangeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> LargeChange<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.LargeChangeProperty, binding);


 // MinimumProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Minimum<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.MinimumProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Minimum<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.MinimumProperty, binding);


 // MaximumProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Maximum<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.MaximumProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Maximum<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.MaximumProperty, binding);


 // PlaceholderTextProperty

/*ValueStyleSetterGenerator*/
public static Style<T> PlaceholderText<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.PlaceholderTextProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PlaceholderText<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.PlaceholderTextProperty, binding);


 // SelectionFlyoutProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SelectionFlyout<T>(this Style<T> style, Avalonia.Controls.Primitives.FlyoutBase value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.SelectionFlyoutProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectionFlyout<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.SelectionFlyoutProperty, binding);


 // SelectionHighlightColorProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SelectionHighlightColor<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.SelectionHighlightColorProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectionHighlightColor<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.SelectionHighlightColorProperty, binding);


 // SmallChangeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SmallChange<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.SmallChangeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SmallChange<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.SmallChangeProperty, binding);


 // SpinButtonPlacementModeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SpinButtonPlacementMode<T>(this Style<T> style, FluentAvalonia.UI.Controls.NumberBoxSpinButtonPlacementMode value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.SpinButtonPlacementModeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SpinButtonPlacementMode<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.SpinButtonPlacementModeProperty, binding);


 // TextReadingOrderProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TextReadingOrder<T>(this Style<T> style, FluentAvalonia.UI.Controls.TextReadingOrder value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.TextReadingOrderProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TextReadingOrder<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.TextReadingOrderProperty, binding);


 // ValidationModeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ValidationMode<T>(this Style<T> style, FluentAvalonia.UI.Controls.NumberBoxValidationMode value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.ValidationModeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ValidationMode<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.ValidationModeProperty, binding);


 // ValueProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Value<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.ValueProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Value<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.ValueProperty, binding);


 // TextAlignmentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TextAlignment<T>(this Style<T> style, Avalonia.Media.TextAlignment value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.TextAlignmentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TextAlignment<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.TextAlignmentProperty, binding);


 // SimpleNumberFormatProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SimpleNumberFormat<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.SimpleNumberFormatProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SimpleNumberFormat<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.SimpleNumberFormatProperty, binding);



}
