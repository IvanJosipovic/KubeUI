using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using DisableContainer = Ursa.Controls.DisableContainer;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class DisableContainerExtensions
{
public static Style<T> Content<T>(this Style<T> style, Avalonia.Input.InputElement value) where T : Ursa.Controls.DisableContainer
=> style._addSetter(Ursa.Controls.DisableContainer.ContentProperty, value);
public static Style<T> Content<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DisableContainer
=> style._addSetter(Ursa.Controls.DisableContainer.ContentProperty, binding);
public static Style<T> DisabledTip<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.DisableContainer
=> style._addSetter(Ursa.Controls.DisableContainer.DisabledTipProperty, value);
public static Style<T> DisabledTip<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DisableContainer
=> style._addSetter(Ursa.Controls.DisableContainer.DisabledTipProperty, binding);
}

