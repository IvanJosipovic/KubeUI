using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DualBadge = Ursa.Controls.DualBadge;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class DualBadgeExtensions
{
public static Style<T> Icon<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.DualBadge
=> style._addSetter(Ursa.Controls.DualBadge.IconProperty, value);
public static Style<T> Icon<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DualBadge
=> style._addSetter(Ursa.Controls.DualBadge.IconProperty, binding);
public static Style<T> IconTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.DualBadge
=> style._addSetter(Ursa.Controls.DualBadge.IconTemplateProperty, value);
public static Style<T> IconTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DualBadge
=> style._addSetter(Ursa.Controls.DualBadge.IconTemplateProperty, binding);
public static Style<T> IconForeground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.DualBadge
=> style._addSetter(Ursa.Controls.DualBadge.IconForegroundProperty, value);
public static Style<T> IconForeground<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DualBadge
=> style._addSetter(Ursa.Controls.DualBadge.IconForegroundProperty, binding);
public static Style<T> HeaderForeground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.DualBadge
=> style._addSetter(Ursa.Controls.DualBadge.HeaderForegroundProperty, value);
public static Style<T> HeaderForeground<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DualBadge
=> style._addSetter(Ursa.Controls.DualBadge.HeaderForegroundProperty, binding);
public static Style<T> HeaderBackground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Ursa.Controls.DualBadge
=> style._addSetter(Ursa.Controls.DualBadge.HeaderBackgroundProperty, value);
public static Style<T> HeaderBackground<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DualBadge
=> style._addSetter(Ursa.Controls.DualBadge.HeaderBackgroundProperty, binding);
}

