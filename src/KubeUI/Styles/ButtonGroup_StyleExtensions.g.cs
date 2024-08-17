using Avalonia.Data;
using Avalonia.Data.Converters;
using ButtonGroup = Ursa.Controls.ButtonGroup;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ButtonGroupExtensions
{
public static Style<T> CommandBinding<T>(this Style<T> style, Avalonia.Data.IBinding value) where T : Ursa.Controls.ButtonGroup
=> style._addSetter(Ursa.Controls.ButtonGroup.CommandBindingProperty, value);
//Skipped CommandBinding because already exist in value setters
public static Style<T> CommandParameterBinding<T>(this Style<T> style, Avalonia.Data.IBinding value) where T : Ursa.Controls.ButtonGroup
=> style._addSetter(Ursa.Controls.ButtonGroup.CommandParameterBindingProperty, value);
//Skipped CommandParameterBinding because already exist in value setters
public static Style<T> ContentBinding<T>(this Style<T> style, Avalonia.Data.IBinding value) where T : Ursa.Controls.ButtonGroup
=> style._addSetter(Ursa.Controls.ButtonGroup.ContentBindingProperty, value);
//Skipped ContentBinding because already exist in value setters
}

