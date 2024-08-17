using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TimePickerPresenter = Ursa.Controls.TimePickerPresenter;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TimePickerPresenterExtensions
{
public static Style<T> NeedsConfirmation<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.TimePickerPresenter
=> style._addSetter(Ursa.Controls.TimePickerPresenter.NeedsConfirmationProperty, value);
public static Style<T> NeedsConfirmation<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerPresenter
=> style._addSetter(Ursa.Controls.TimePickerPresenter.NeedsConfirmationProperty, binding);
public static Style<T> MinuteIncrement<T>(this Style<T> style, System.Int32 value) where T : Ursa.Controls.TimePickerPresenter
=> style._addSetter(Ursa.Controls.TimePickerPresenter.MinuteIncrementProperty, value);
public static Style<T> MinuteIncrement<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerPresenter
=> style._addSetter(Ursa.Controls.TimePickerPresenter.MinuteIncrementProperty, binding);
public static Style<T> SecondIncrement<T>(this Style<T> style, System.Int32 value) where T : Ursa.Controls.TimePickerPresenter
=> style._addSetter(Ursa.Controls.TimePickerPresenter.SecondIncrementProperty, value);
public static Style<T> SecondIncrement<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerPresenter
=> style._addSetter(Ursa.Controls.TimePickerPresenter.SecondIncrementProperty, binding);
public static Style<T> Time<T>(this Style<T> style, System.Nullable<System.TimeSpan> value) where T : Ursa.Controls.TimePickerPresenter
=> style._addSetter(Ursa.Controls.TimePickerPresenter.TimeProperty, value);
public static Style<T> Time<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerPresenter
=> style._addSetter(Ursa.Controls.TimePickerPresenter.TimeProperty, binding);
public static Style<T> PanelFormat<T>(this Style<T> style, System.String value) where T : Ursa.Controls.TimePickerPresenter
=> style._addSetter(Ursa.Controls.TimePickerPresenter.PanelFormatProperty, value);
public static Style<T> PanelFormat<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimePickerPresenter
=> style._addSetter(Ursa.Controls.TimePickerPresenter.PanelFormatProperty, binding);
}

