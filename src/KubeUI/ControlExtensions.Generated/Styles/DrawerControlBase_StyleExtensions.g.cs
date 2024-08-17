using Avalonia.Data;
using Avalonia.Data.Converters;
using DrawerControlBase = Ursa.Controls.DrawerControlBase;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Common;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class DrawerControlBaseExtensions
{
public static Style<T> Position<T>(this Style<T> style, Ursa.Common.Position value) where T : Ursa.Controls.DrawerControlBase
=> style._addSetter(Ursa.Controls.DrawerControlBase.PositionProperty, value);
public static Style<T> Position<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DrawerControlBase
=> style._addSetter(Ursa.Controls.DrawerControlBase.PositionProperty, binding);
public static Style<T> IsOpen<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.DrawerControlBase
=> style._addSetter(Ursa.Controls.DrawerControlBase.IsOpenProperty, value);
public static Style<T> IsOpen<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DrawerControlBase
=> style._addSetter(Ursa.Controls.DrawerControlBase.IsOpenProperty, binding);
public static Style<T> IsCloseButtonVisible<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.DrawerControlBase
=> style._addSetter(Ursa.Controls.DrawerControlBase.IsCloseButtonVisibleProperty, value);
public static Style<T> IsCloseButtonVisible<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.DrawerControlBase
=> style._addSetter(Ursa.Controls.DrawerControlBase.IsCloseButtonVisibleProperty, binding);
}

