using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TimelineItem = Ursa.Controls.TimelineItem;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TimelineItemExtensions
{
public static Style<T> Icon<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.IconProperty, value);
public static Style<T> Icon<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.IconProperty, binding);
public static Style<T> IconTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.IconTemplateProperty, value);
public static Style<T> IconTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.IconTemplateProperty, binding);
public static Style<T> Type<T>(this Style<T> style, Ursa.Controls.TimelineItemType value) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.TypeProperty, value);
public static Style<T> Type<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.TypeProperty, binding);
public static Style<T> Position<T>(this Style<T> style, Ursa.Controls.TimelineItemPosition value) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.PositionProperty, value);
public static Style<T> Position<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.PositionProperty, binding);
public static Style<T> Time<T>(this Style<T> style, System.DateTime value) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.TimeProperty, value);
public static Style<T> Time<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.TimeProperty, binding);
public static Style<T> TimeFormat<T>(this Style<T> style, System.String value) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.TimeFormatProperty, value);
public static Style<T> TimeFormat<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TimelineItem
=> style._addSetter(Ursa.Controls.TimelineItem.TimeFormatProperty, binding);
}

