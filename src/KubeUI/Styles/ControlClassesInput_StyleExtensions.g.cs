using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using ControlClassesInput = Ursa.Controls.ControlClassesInput;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ControlClassesInputExtensions
{
public static Style<T> Target<T>(this Style<T> style, Avalonia.Controls.Control value) where T : Ursa.Controls.ControlClassesInput
=> style._addSetter(Ursa.Controls.ControlClassesInput.TargetProperty, value);
public static Style<T> Target<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ControlClassesInput
=> style._addSetter(Ursa.Controls.ControlClassesInput.TargetProperty, binding);
public static Style<T> Separator<T>(this Style<T> style, System.String value) where T : Ursa.Controls.ControlClassesInput
=> style._addSetter(Ursa.Controls.ControlClassesInput.SeparatorProperty, value);
public static Style<T> Separator<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ControlClassesInput
=> style._addSetter(Ursa.Controls.ControlClassesInput.SeparatorProperty, binding);
}

