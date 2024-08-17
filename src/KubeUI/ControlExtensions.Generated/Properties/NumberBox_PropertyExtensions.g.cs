#nullable enable
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using NumberBox = FluentAvalonia.UI.Controls.NumberBox;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class NumberBoxExtensions
{
public static T AcceptsExpression<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.AcceptsExpressionProperty, binding);
public static T AcceptsExpression<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.AcceptsExpressionProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AcceptsExpression<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.AcceptsExpressionProperty, func, onChanged, expression);
public static T AcceptsExpression<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.AcceptsExpressionProperty, ps, () => control.AcceptsExpression = value, bindingMode, converter, bindingSource);
public static T AcceptsExpression<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.AcceptsExpressionProperty, ps, () => control.AcceptsExpression = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Description<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.DescriptionProperty, binding);
public static T Description<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.DescriptionProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Description<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.DescriptionProperty, func, onChanged, expression);
public static T Description<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.DescriptionProperty, ps, () => control.Description = value, bindingMode, converter, bindingSource);
public static T Description<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.DescriptionProperty, ps, () => control.Description = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Header<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.HeaderProperty, binding);
public static T Header<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.HeaderProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Header<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.HeaderProperty, func, onChanged, expression);
public static T Header<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.HeaderProperty, ps, () => control.Header = value, bindingMode, converter, bindingSource);
public static T Header<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.HeaderProperty, ps, () => control.Header = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HeaderTemplate<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.HeaderTemplateProperty, binding);
public static T HeaderTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.HeaderTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HeaderTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.HeaderTemplateProperty, func, onChanged, expression);
public static T HeaderTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.HeaderTemplateProperty, ps, () => control.HeaderTemplate = value, bindingMode, converter, bindingSource);
public static T HeaderTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.HeaderTemplateProperty, ps, () => control.HeaderTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsWrapEnabled<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.IsWrapEnabledProperty, binding);
public static T IsWrapEnabled<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.IsWrapEnabledProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsWrapEnabled<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.IsWrapEnabledProperty, func, onChanged, expression);
public static T IsWrapEnabled<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.IsWrapEnabledProperty, ps, () => control.IsWrapEnabled = value, bindingMode, converter, bindingSource);
public static T IsWrapEnabled<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.IsWrapEnabledProperty, ps, () => control.IsWrapEnabled = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T LargeChange<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.LargeChangeProperty, binding);
public static T LargeChange<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.LargeChangeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T LargeChange<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.LargeChangeProperty, func, onChanged, expression);
public static T LargeChange<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.LargeChangeProperty, ps, () => control.LargeChange = value, bindingMode, converter, bindingSource);
public static T LargeChange<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.LargeChangeProperty, ps, () => control.LargeChange = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Minimum<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.MinimumProperty, binding);
public static T Minimum<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.MinimumProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Minimum<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.MinimumProperty, func, onChanged, expression);
public static T Minimum<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.MinimumProperty, ps, () => control.Minimum = value, bindingMode, converter, bindingSource);
public static T Minimum<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.MinimumProperty, ps, () => control.Minimum = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Maximum<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.MaximumProperty, binding);
public static T Maximum<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.MaximumProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Maximum<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.MaximumProperty, func, onChanged, expression);
public static T Maximum<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.MaximumProperty, ps, () => control.Maximum = value, bindingMode, converter, bindingSource);
public static T Maximum<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.MaximumProperty, ps, () => control.Maximum = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T PlaceholderText<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.PlaceholderTextProperty, binding);
public static T PlaceholderText<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.PlaceholderTextProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T PlaceholderText<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.PlaceholderTextProperty, func, onChanged, expression);
public static T PlaceholderText<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.PlaceholderTextProperty, ps, () => control.PlaceholderText = value, bindingMode, converter, bindingSource);
public static T PlaceholderText<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.PlaceholderTextProperty, ps, () => control.PlaceholderText = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectionFlyout<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SelectionFlyoutProperty, binding);
public static T SelectionFlyout<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SelectionFlyoutProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectionFlyout<T>(this T control, Func<Avalonia.Controls.Primitives.FlyoutBase> func, Action<Avalonia.Controls.Primitives.FlyoutBase>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SelectionFlyoutProperty, func, onChanged, expression);
public static T SelectionFlyout<T>(this T control, Avalonia.Controls.Primitives.FlyoutBase value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.SelectionFlyoutProperty, ps, () => control.SelectionFlyout = value, bindingMode, converter, bindingSource);
public static T SelectionFlyout<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Primitives.FlyoutBase> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.SelectionFlyoutProperty, ps, () => control.SelectionFlyout = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SelectionHighlightColor<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SelectionHighlightColorProperty, binding);
public static T SelectionHighlightColor<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SelectionHighlightColorProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SelectionHighlightColor<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SelectionHighlightColorProperty, func, onChanged, expression);
public static T SelectionHighlightColor<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.SelectionHighlightColorProperty, ps, () => control.SelectionHighlightColor = value, bindingMode, converter, bindingSource);
public static T SelectionHighlightColor<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.SelectionHighlightColorProperty, ps, () => control.SelectionHighlightColor = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SmallChange<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SmallChangeProperty, binding);
public static T SmallChange<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SmallChangeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SmallChange<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SmallChangeProperty, func, onChanged, expression);
public static T SmallChange<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.SmallChangeProperty, ps, () => control.SmallChange = value, bindingMode, converter, bindingSource);
public static T SmallChange<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.SmallChangeProperty, ps, () => control.SmallChange = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SpinButtonPlacementMode<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SpinButtonPlacementModeProperty, binding);
public static T SpinButtonPlacementMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SpinButtonPlacementModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SpinButtonPlacementMode<T>(this T control, Func<FluentAvalonia.UI.Controls.NumberBoxSpinButtonPlacementMode> func, Action<FluentAvalonia.UI.Controls.NumberBoxSpinButtonPlacementMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SpinButtonPlacementModeProperty, func, onChanged, expression);
public static T SpinButtonPlacementMode<T>(this T control, FluentAvalonia.UI.Controls.NumberBoxSpinButtonPlacementMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.SpinButtonPlacementModeProperty, ps, () => control.SpinButtonPlacementMode = value, bindingMode, converter, bindingSource);
public static T SpinButtonPlacementMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.NumberBoxSpinButtonPlacementMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.SpinButtonPlacementModeProperty, ps, () => control.SpinButtonPlacementMode = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Text<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.TextProperty, binding);
public static T Text<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.TextProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Text<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.TextProperty, func, onChanged, expression);
public static T Text<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.TextProperty, ps, () => control.Text = value, bindingMode, converter, bindingSource);
public static T Text<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.TextProperty, ps, () => control.Text = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TextReadingOrder<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.TextReadingOrderProperty, binding);
public static T TextReadingOrder<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.TextReadingOrderProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TextReadingOrder<T>(this T control, Func<FluentAvalonia.UI.Controls.TextReadingOrder> func, Action<FluentAvalonia.UI.Controls.TextReadingOrder>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.TextReadingOrderProperty, func, onChanged, expression);
public static T TextReadingOrder<T>(this T control, FluentAvalonia.UI.Controls.TextReadingOrder value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.TextReadingOrderProperty, ps, () => control.TextReadingOrder = value, bindingMode, converter, bindingSource);
public static T TextReadingOrder<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.TextReadingOrder> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.TextReadingOrderProperty, ps, () => control.TextReadingOrder = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ValidationMode<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.ValidationModeProperty, binding);
public static T ValidationMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.ValidationModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ValidationMode<T>(this T control, Func<FluentAvalonia.UI.Controls.NumberBoxValidationMode> func, Action<FluentAvalonia.UI.Controls.NumberBoxValidationMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.ValidationModeProperty, func, onChanged, expression);
public static T ValidationMode<T>(this T control, FluentAvalonia.UI.Controls.NumberBoxValidationMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.ValidationModeProperty, ps, () => control.ValidationMode = value, bindingMode, converter, bindingSource);
public static T ValidationMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, FluentAvalonia.UI.Controls.NumberBoxValidationMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.ValidationModeProperty, ps, () => control.ValidationMode = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Value<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.ValueProperty, binding);
public static T Value<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.ValueProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Value<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.ValueProperty, func, onChanged, expression);
public static T Value<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.ValueProperty, ps, () => control.Value = value, bindingMode, converter, bindingSource);
public static T Value<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.ValueProperty, ps, () => control.Value = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TextAlignment<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.TextAlignmentProperty, binding);
public static T TextAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.TextAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TextAlignment<T>(this T control, Func<Avalonia.Media.TextAlignment> func, Action<Avalonia.Media.TextAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.TextAlignmentProperty, func, onChanged, expression);
public static T TextAlignment<T>(this T control, Avalonia.Media.TextAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.TextAlignmentProperty, ps, () => control.TextAlignment = value, bindingMode, converter, bindingSource);
public static T TextAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.TextAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.TextAlignmentProperty, ps, () => control.TextAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T SimpleNumberFormat<T>(this T control, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SimpleNumberFormatProperty, binding);
public static T SimpleNumberFormat<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SimpleNumberFormatProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T SimpleNumberFormat<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : FluentAvalonia.UI.Controls.NumberBox
   => control._set(FluentAvalonia.UI.Controls.NumberBox.SimpleNumberFormatProperty, func, onChanged, expression);
public static T SimpleNumberFormat<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.SimpleNumberFormatProperty, ps, () => control.SimpleNumberFormat = value, bindingMode, converter, bindingSource);
public static T SimpleNumberFormat<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : FluentAvalonia.UI.Controls.NumberBox
=> control._setEx(FluentAvalonia.UI.Controls.NumberBox.SimpleNumberFormatProperty, ps, () => control.SimpleNumberFormat = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

