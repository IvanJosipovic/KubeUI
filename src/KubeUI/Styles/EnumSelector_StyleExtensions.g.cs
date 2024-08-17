using Avalonia.Data;
using Avalonia.Data.Converters;
using EnumSelector = Ursa.Controls.EnumSelector;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class EnumSelectorExtensions
{
public static Style<T> EnumType<T>(this Style<T> style, System.Type value) where T : Ursa.Controls.EnumSelector
=> style._addSetter(Ursa.Controls.EnumSelector.EnumTypeProperty, value);
public static Style<T> EnumType<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.EnumSelector
=> style._addSetter(Ursa.Controls.EnumSelector.EnumTypeProperty, binding);
public static Style<T> Value<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.EnumSelector
=> style._addSetter(Ursa.Controls.EnumSelector.ValueProperty, value);
public static Style<T> Value<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.EnumSelector
=> style._addSetter(Ursa.Controls.EnumSelector.ValueProperty, binding);
public static Style<T> DisplayDescription<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.EnumSelector
=> style._addSetter(Ursa.Controls.EnumSelector.DisplayDescriptionProperty, value);
public static Style<T> DisplayDescription<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.EnumSelector
=> style._addSetter(Ursa.Controls.EnumSelector.DisplayDescriptionProperty, binding);
}

