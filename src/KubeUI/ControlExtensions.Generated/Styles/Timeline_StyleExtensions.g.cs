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
public static Style<T> IconMemberBinding<T>(this Style<T> style, Avalonia.Data.IBinding value) where T : Ursa.Controls.Timeline
=> style._addSetter(Ursa.Controls.Timeline.IconMemberBindingProperty, value);
//Skipped IconMemberBinding because already exist in value setters
public static Style<T> HeaderMemberBinding<T>(this Style<T> style, Avalonia.Data.IBinding value) where T : Ursa.Controls.Timeline
=> style._addSetter(Ursa.Controls.Timeline.HeaderMemberBindingProperty, value);
//Skipped HeaderMemberBinding because already exist in value setters
public static Style<T> ContentMemberBinding<T>(this Style<T> style, Avalonia.Data.IBinding value) where T : Ursa.Controls.Timeline
=> style._addSetter(Ursa.Controls.Timeline.ContentMemberBindingProperty, value);
//Skipped ContentMemberBinding because already exist in value setters
public static Style<T> IconTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.Timeline
=> style._addSetter(Ursa.Controls.Timeline.IconTemplateProperty, value);
public static Style<T> IconTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Timeline
=> style._addSetter(Ursa.Controls.Timeline.IconTemplateProperty, binding);
public static Style<T> DescriptionTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.Timeline
=> style._addSetter(Ursa.Controls.Timeline.DescriptionTemplateProperty, value);
public static Style<T> DescriptionTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Timeline
=> style._addSetter(Ursa.Controls.Timeline.DescriptionTemplateProperty, binding);
public static Style<T> TimeMemberBinding<T>(this Style<T> style, Avalonia.Data.IBinding value) where T : Ursa.Controls.Timeline
=> style._addSetter(Ursa.Controls.Timeline.TimeMemberBindingProperty, value);
//Skipped TimeMemberBinding because already exist in value setters
public static Style<T> TimeFormat<T>(this Style<T> style, System.String value) where T : Ursa.Controls.Timeline
=> style._addSetter(Ursa.Controls.Timeline.TimeFormatProperty, value);
public static Style<T> TimeFormat<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Timeline
=> style._addSetter(Ursa.Controls.Timeline.TimeFormatProperty, binding);
public static Style<T> Mode<T>(this Style<T> style, Ursa.Controls.TimelineDisplayMode value) where T : Ursa.Controls.Timeline
=> style._addSetter(Ursa.Controls.Timeline.ModeProperty, value);
public static Style<T> Mode<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Timeline
=> style._addSetter(Ursa.Controls.Timeline.ModeProperty, binding);
}

