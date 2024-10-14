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
public static partial class OverloadViewer_MarkupExtensions
{
//================= Properties ======================//
 // TextProperty

/*BindFromExpressionSetterGenerator*/
public static T Text<T>(this T control, Func<System.String> func, Action<System.String>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.CodeCompletion.OverloadViewer
   => control._set(AvaloniaEdit.CodeCompletion.OverloadViewer.TextProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Text<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.CodeCompletion.OverloadViewer
=> control._setEx(AvaloniaEdit.CodeCompletion.OverloadViewer.TextProperty, ps, () => control.Text = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Text<T>(this T control, IBinding binding) where T : AvaloniaEdit.CodeCompletion.OverloadViewer
   => control._set(AvaloniaEdit.CodeCompletion.OverloadViewer.TextProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Text<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.CodeCompletion.OverloadViewer
   => control._set(AvaloniaEdit.CodeCompletion.OverloadViewer.TextProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Text<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.CodeCompletion.OverloadViewer
=> control._setEx(AvaloniaEdit.CodeCompletion.OverloadViewer.TextProperty, ps, () => control.Text = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ProviderProperty

/*BindFromExpressionSetterGenerator*/
public static T Provider<T>(this T control, Func<AvaloniaEdit.CodeCompletion.IOverloadProvider> func, Action<AvaloniaEdit.CodeCompletion.IOverloadProvider>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : AvaloniaEdit.CodeCompletion.OverloadViewer
   => control._set(AvaloniaEdit.CodeCompletion.OverloadViewer.ProviderProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Provider<T>(this T control, AvaloniaEdit.CodeCompletion.IOverloadProvider value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.CodeCompletion.OverloadViewer
=> control._setEx(AvaloniaEdit.CodeCompletion.OverloadViewer.ProviderProperty, ps, () => control.Provider = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Provider<T>(this T control, IBinding binding) where T : AvaloniaEdit.CodeCompletion.OverloadViewer
   => control._set(AvaloniaEdit.CodeCompletion.OverloadViewer.ProviderProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Provider<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : AvaloniaEdit.CodeCompletion.OverloadViewer
   => control._set(AvaloniaEdit.CodeCompletion.OverloadViewer.ProviderProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Provider<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, AvaloniaEdit.CodeCompletion.IOverloadProvider> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : AvaloniaEdit.CodeCompletion.OverloadViewer
=> control._setEx(AvaloniaEdit.CodeCompletion.OverloadViewer.ProviderProperty, ps, () => control.Provider = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // TextProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Text<T>(this Style<T> style, System.String value) where T : AvaloniaEdit.CodeCompletion.OverloadViewer
=> style._addSetter(AvaloniaEdit.CodeCompletion.OverloadViewer.TextProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Text<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.CodeCompletion.OverloadViewer
=> style._addSetter(AvaloniaEdit.CodeCompletion.OverloadViewer.TextProperty, binding);


 // ProviderProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Provider<T>(this Style<T> style, AvaloniaEdit.CodeCompletion.IOverloadProvider value) where T : AvaloniaEdit.CodeCompletion.OverloadViewer
=> style._addSetter(AvaloniaEdit.CodeCompletion.OverloadViewer.ProviderProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Provider<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.CodeCompletion.OverloadViewer
=> style._addSetter(AvaloniaEdit.CodeCompletion.OverloadViewer.ProviderProperty, binding);



}
