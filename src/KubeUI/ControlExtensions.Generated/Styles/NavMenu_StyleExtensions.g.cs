using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using NavMenu = Ursa.Controls.NavMenu;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class NavMenuExtensions
{
public static Style<T> SelectedItem<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.SelectedItemProperty, value);
public static Style<T> SelectedItem<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.SelectedItemProperty, binding);
public static Style<T> IconBinding<T>(this Style<T> style, Avalonia.Data.IBinding value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.IconBindingProperty, value);
//Skipped IconBinding because already exist in value setters
public static Style<T> HeaderBinding<T>(this Style<T> style, Avalonia.Data.IBinding value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.HeaderBindingProperty, value);
//Skipped HeaderBinding because already exist in value setters
public static Style<T> SubMenuBinding<T>(this Style<T> style, Avalonia.Data.IBinding value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.SubMenuBindingProperty, value);
//Skipped SubMenuBinding because already exist in value setters
public static Style<T> CommandBinding<T>(this Style<T> style, Avalonia.Data.IBinding value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.CommandBindingProperty, value);
//Skipped CommandBinding because already exist in value setters
public static Style<T> HeaderTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.HeaderTemplateProperty, value);
public static Style<T> HeaderTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.HeaderTemplateProperty, binding);
public static Style<T> IconTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.IconTemplateProperty, value);
public static Style<T> IconTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.IconTemplateProperty, binding);
public static Style<T> SubMenuIndent<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.SubMenuIndentProperty, value);
public static Style<T> SubMenuIndent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.SubMenuIndentProperty, binding);
public static Style<T> IsHorizontalCollapsed<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.IsHorizontalCollapsedProperty, value);
public static Style<T> IsHorizontalCollapsed<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.IsHorizontalCollapsedProperty, binding);
public static Style<T> Header<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.HeaderProperty, value);
public static Style<T> Header<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.HeaderProperty, binding);
public static Style<T> Footer<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.FooterProperty, value);
public static Style<T> Footer<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.FooterProperty, binding);
public static Style<T> ExpandWidth<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.ExpandWidthProperty, value);
public static Style<T> ExpandWidth<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.ExpandWidthProperty, binding);
public static Style<T> CollapseWidth<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.CollapseWidthProperty, value);
public static Style<T> CollapseWidth<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.NavMenu
=> style._addSetter(Ursa.Controls.NavMenu.CollapseWidthProperty, binding);
}

