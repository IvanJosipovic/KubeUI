using Avalonia.Data;
using Avalonia.Data.Converters;
using DateRangePicker = Ursa.Controls.DateRangePicker;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class DateRangePickerExtensions
{
public static Style<T> SelectedStartDate<T>(this Style<T> style, System.Nullable<System.DateTime> value) where T : Ursa.Controls.DateRangePicker
=> style._addSetter(Ursa.Controls.DateRangePicker.SelectedStartDateProperty, value);
public static Style<T> SelectedStartDate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DateRangePicker
=> style._addSetter(Ursa.Controls.DateRangePicker.SelectedStartDateProperty, binding);
public static Style<T> SelectedEndDate<T>(this Style<T> style, System.Nullable<System.DateTime> value) where T : Ursa.Controls.DateRangePicker
=> style._addSetter(Ursa.Controls.DateRangePicker.SelectedEndDateProperty, value);
public static Style<T> SelectedEndDate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DateRangePicker
=> style._addSetter(Ursa.Controls.DateRangePicker.SelectedEndDateProperty, binding);
public static Style<T> EnableMonthSync<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.DateRangePicker
=> style._addSetter(Ursa.Controls.DateRangePicker.EnableMonthSyncProperty, value);
public static Style<T> EnableMonthSync<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DateRangePicker
=> style._addSetter(Ursa.Controls.DateRangePicker.EnableMonthSyncProperty, binding);
}

