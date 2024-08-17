#nullable enable
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Timeline = Ursa.Controls.Timeline;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TimelineExtensions
{
public static T IconMemberBinding<T>(this T control, IBinding binding) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.IconMemberBindingProperty, binding);
public static T IconMemberBinding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.IconMemberBindingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconMemberBinding<T>(this T control, Func<Avalonia.Data.IBinding> func, Action<Avalonia.Data.IBinding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.IconMemberBindingProperty, func, onChanged, expression);
public static T IconMemberBinding<T>(this T control, Avalonia.Data.IBinding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Timeline
=> control._setEx(Ursa.Controls.Timeline.IconMemberBindingProperty, ps, () => control.IconMemberBinding = value, bindingMode, converter, bindingSource);
public static T IconMemberBinding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.IBinding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Timeline
=> control._setEx(Ursa.Controls.Timeline.IconMemberBindingProperty, ps, () => control.IconMemberBinding = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T HeaderMemberBinding<T>(this T control, IBinding binding) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.HeaderMemberBindingProperty, binding);
public static T HeaderMemberBinding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.HeaderMemberBindingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T HeaderMemberBinding<T>(this T control, Func<Avalonia.Data.IBinding> func, Action<Avalonia.Data.IBinding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.HeaderMemberBindingProperty, func, onChanged, expression);
public static T HeaderMemberBinding<T>(this T control, Avalonia.Data.IBinding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Timeline
=> control._setEx(Ursa.Controls.Timeline.HeaderMemberBindingProperty, ps, () => control.HeaderMemberBinding = value, bindingMode, converter, bindingSource);
public static T HeaderMemberBinding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.IBinding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Timeline
=> control._setEx(Ursa.Controls.Timeline.HeaderMemberBindingProperty, ps, () => control.HeaderMemberBinding = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T ContentMemberBinding<T>(this T control, IBinding binding) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.ContentMemberBindingProperty, binding);
public static T ContentMemberBinding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.ContentMemberBindingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T ContentMemberBinding<T>(this T control, Func<Avalonia.Data.IBinding> func, Action<Avalonia.Data.IBinding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.ContentMemberBindingProperty, func, onChanged, expression);
public static T ContentMemberBinding<T>(this T control, Avalonia.Data.IBinding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Timeline
=> control._setEx(Ursa.Controls.Timeline.ContentMemberBindingProperty, ps, () => control.ContentMemberBinding = value, bindingMode, converter, bindingSource);
public static T ContentMemberBinding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.IBinding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Timeline
=> control._setEx(Ursa.Controls.Timeline.ContentMemberBindingProperty, ps, () => control.ContentMemberBinding = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T IconTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.IconTemplateProperty, binding);
public static T IconTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.IconTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T IconTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.IconTemplateProperty, func, onChanged, expression);
public static T IconTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Timeline
=> control._setEx(Ursa.Controls.Timeline.IconTemplateProperty, ps, () => control.IconTemplate = value, bindingMode, converter, bindingSource);
public static T IconTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Timeline
=> control._setEx(Ursa.Controls.Timeline.IconTemplateProperty, ps, () => control.IconTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T DescriptionTemplate<T>(this T control, IBinding binding) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.DescriptionTemplateProperty, binding);
public static T DescriptionTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.DescriptionTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T DescriptionTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate> func, Action<Avalonia.Controls.Templates.IDataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.DescriptionTemplateProperty, func, onChanged, expression);
public static T DescriptionTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Timeline
=> control._setEx(Ursa.Controls.Timeline.DescriptionTemplateProperty, ps, () => control.DescriptionTemplate = value, bindingMode, converter, bindingSource);
public static T DescriptionTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Timeline
=> control._setEx(Ursa.Controls.Timeline.DescriptionTemplateProperty, ps, () => control.DescriptionTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TimeMemberBinding<T>(this T control, IBinding binding) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.TimeMemberBindingProperty, binding);
public static T TimeMemberBinding<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.TimeMemberBindingProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TimeMemberBinding<T>(this T control, Func<Avalonia.Data.IBinding> func, Action<Avalonia.Data.IBinding>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.TimeMemberBindingProperty, func, onChanged, expression);
public static T TimeMemberBinding<T>(this T control, Avalonia.Data.IBinding value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Timeline
=> control._setEx(Ursa.Controls.Timeline.TimeMemberBindingProperty, ps, () => control.TimeMemberBinding = value, bindingMode, converter, bindingSource);
public static T TimeMemberBinding<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Data.IBinding> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Timeline
=> control._setEx(Ursa.Controls.Timeline.TimeMemberBindingProperty, ps, () => control.TimeMemberBinding = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T TimeFormat<T>(this T control, IBinding binding) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.TimeFormatProperty, binding);
public static T TimeFormat<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.TimeFormatProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T TimeFormat<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.TimeFormatProperty, func, onChanged, expression);
public static T TimeFormat<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Timeline
=> control._setEx(Ursa.Controls.Timeline.TimeFormatProperty, ps, () => control.TimeFormat = value, bindingMode, converter, bindingSource);
public static T TimeFormat<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Timeline
=> control._setEx(Ursa.Controls.Timeline.TimeFormatProperty, ps, () => control.TimeFormat = converter.TryConvert(value), bindingMode, converter, bindingSource);
public static T Mode<T>(this T control, IBinding binding) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.ModeProperty, binding);
public static T Mode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.ModeProperty, avaloniaProperty, bindingMode, converter, overrideView);
public static T Mode<T>(this T control, Func<Ursa.Controls.TimelineDisplayMode> func, Action<Ursa.Controls.TimelineDisplayMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Ursa.Controls.Timeline
   => control._set(Ursa.Controls.Timeline.ModeProperty, func, onChanged, expression);
public static T Mode<T>(this T control, Ursa.Controls.TimelineDisplayMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Timeline
=> control._setEx(Ursa.Controls.Timeline.ModeProperty, ps, () => control.Mode = value, bindingMode, converter, bindingSource);
public static T Mode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Ursa.Controls.TimelineDisplayMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Ursa.Controls.Timeline
=> control._setEx(Ursa.Controls.Timeline.ModeProperty, ps, () => control.Mode = converter.TryConvert(value), bindingMode, converter, bindingSource);
}

