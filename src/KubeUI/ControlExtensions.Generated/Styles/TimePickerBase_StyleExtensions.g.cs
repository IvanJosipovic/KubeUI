using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TimePickerBase = Ursa.Controls.TimePickerBase;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TimePickerBaseExtensions
{
public static Style<T> DisplayFormat<T>(this Style<T> style, System.String value) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.DisplayFormatProperty, value);
public static Style<T> DisplayFormat<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.DisplayFormatProperty, binding);
public static Style<T> PanelFormat<T>(this Style<T> style, System.String value) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.PanelFormatProperty, value);
public static Style<T> PanelFormat<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.PanelFormatProperty, binding);
public static Style<T> NeedConfirmation<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.NeedConfirmationProperty, value);
public static Style<T> NeedConfirmation<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.NeedConfirmationProperty, binding);
public static Style<T> InnerLeftContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.InnerLeftContentProperty, value);
public static Style<T> InnerLeftContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.InnerLeftContentProperty, binding);
public static Style<T> InnerRightContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.InnerRightContentProperty, value);
public static Style<T> InnerRightContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.InnerRightContentProperty, binding);
public static Style<T> PopupInnerTopContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.PopupInnerTopContentProperty, value);
public static Style<T> PopupInnerTopContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.PopupInnerTopContentProperty, binding);
public static Style<T> PopupInnerBottomContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.PopupInnerBottomContentProperty, value);
public static Style<T> PopupInnerBottomContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.PopupInnerBottomContentProperty, binding);
public static Style<T> IsDropdownOpen<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.IsDropdownOpenProperty, value);
public static Style<T> IsDropdownOpen<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.IsDropdownOpenProperty, binding);
public static Style<T> IsReadonly<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.IsReadonlyProperty, value);
public static Style<T> IsReadonly<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerBase
=> style._addSetter(Ursa.Controls.TimePickerBase.IsReadonlyProperty, binding);
}

