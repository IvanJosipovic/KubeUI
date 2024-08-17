#nullable enable
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using DataGridRow = Avalonia.Controls.DataGridRow;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DataGridRowExtensions
{
public static T Header<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridRow
   => control._set(Avalonia.Controls.DataGridRow.HeaderProperty, binding);
public static T Header<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridRow
   => control._set(Avalonia.Controls.DataGridRow.HeaderProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Header<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridRow
   => control._set(Avalonia.Controls.DataGridRow.HeaderProperty, func, onChanged, expression);
public static T Header<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRow
=> control._setEx(Avalonia.Controls.DataGridRow.HeaderProperty, ps, () => control.Header = value, bindingMode, converter, bindingSource);
public static T Header<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRow
=> control._setEx(Avalonia.Controls.DataGridRow.HeaderProperty, ps, () => control.Header = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IsSelected<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridRow
   => control._set(Avalonia.Controls.DataGridRow.IsSelectedProperty, binding);
public static T IsSelected<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridRow
   => control._set(Avalonia.Controls.DataGridRow.IsSelectedProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IsSelected<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridRow
   => control._set(Avalonia.Controls.DataGridRow.IsSelectedProperty, func, onChanged, expression);
public static T IsSelected<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRow
=> control._setEx(Avalonia.Controls.DataGridRow.IsSelectedProperty, ps, () => control.IsSelected = value, bindingMode, converter, bindingSource);
public static T IsSelected<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRow
=> control._setEx(Avalonia.Controls.DataGridRow.IsSelectedProperty, ps, () => control.IsSelected = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T DetailsTemplate<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridRow
   => control._set(Avalonia.Controls.DataGridRow.DetailsTemplateProperty, binding);
public static T DetailsTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridRow
   => control._set(Avalonia.Controls.DataGridRow.DetailsTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T DetailsTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridRow
   => control._set(Avalonia.Controls.DataGridRow.DetailsTemplateProperty, func, onChanged, expression);
public static T DetailsTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRow
=> control._setEx(Avalonia.Controls.DataGridRow.DetailsTemplateProperty, ps, () => control.DetailsTemplate = value, bindingMode, converter, bindingSource);
public static T DetailsTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRow
=> control._setEx(Avalonia.Controls.DataGridRow.DetailsTemplateProperty, ps, () => control.DetailsTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T AreDetailsVisible<T>(this T control, IBinding binding) where T : Avalonia.Controls.DataGridRow
   => control._set(Avalonia.Controls.DataGridRow.AreDetailsVisibleProperty, binding);
public static T AreDetailsVisible<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Avalonia.Controls.DataGridRow
   => control._set(Avalonia.Controls.DataGridRow.AreDetailsVisibleProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T AreDetailsVisible<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Avalonia.Controls.DataGridRow
   => control._set(Avalonia.Controls.DataGridRow.AreDetailsVisibleProperty, func, onChanged, expression);
public static T AreDetailsVisible<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRow
=> control._setEx(Avalonia.Controls.DataGridRow.AreDetailsVisibleProperty, ps, () => control.AreDetailsVisible = value, bindingMode, converter, bindingSource);
public static T AreDetailsVisible<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Avalonia.Controls.DataGridRow
=> control._setEx(Avalonia.Controls.DataGridRow.AreDetailsVisibleProperty, ps, () => control.AreDetailsVisible = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

