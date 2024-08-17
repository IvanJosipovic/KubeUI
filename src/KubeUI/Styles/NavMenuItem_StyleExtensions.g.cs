using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using NavMenuItem = Ursa.Controls.NavMenuItem;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class NavMenuItemExtensions
{
public static Style<T> Icon<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IconProperty, value);
public static Style<T> Icon<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IconProperty, binding);
public static Style<T> IconTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IconTemplateProperty, value);
public static Style<T> IconTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IconTemplateProperty, binding);
public static Style<T> Command<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.CommandProperty, value);
public static Style<T> Command<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.CommandProperty, binding);
public static Style<T> CommandParameter<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.CommandParameterProperty, value);
public static Style<T> CommandParameter<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.CommandParameterProperty, binding);
public static Style<T> IsSelected<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IsSelectedProperty, value);
public static Style<T> IsSelected<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IsSelectedProperty, binding);
public static Style<T> IsHorizontalCollapsed<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IsHorizontalCollapsedProperty, value);
public static Style<T> IsHorizontalCollapsed<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IsHorizontalCollapsedProperty, binding);
public static Style<T> IsVerticalCollapsed<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IsVerticalCollapsedProperty, value);
public static Style<T> IsVerticalCollapsed<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IsVerticalCollapsedProperty, binding);
public static Style<T> SubMenuIndent<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.SubMenuIndentProperty, value);
public static Style<T> SubMenuIndent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.SubMenuIndentProperty, binding);
public static Style<T> IsSeparator<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IsSeparatorProperty, value);
public static Style<T> IsSeparator<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenuItem
=> style._addSetter(Ursa.Controls.NavMenuItem.IsSeparatorProperty, binding);
}

