using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using BreadcrumbItem = Ursa.Controls.BreadcrumbItem;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class BreadcrumbItemExtensions
{
public static Style<T> Separator<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.BreadcrumbItem
=> style._addSetter(Ursa.Controls.BreadcrumbItem.SeparatorProperty, value);
public static Style<T> Separator<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.BreadcrumbItem
=> style._addSetter(Ursa.Controls.BreadcrumbItem.SeparatorProperty, binding);
public static Style<T> Icon<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.BreadcrumbItem
=> style._addSetter(Ursa.Controls.BreadcrumbItem.IconProperty, value);
public static Style<T> Icon<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.BreadcrumbItem
=> style._addSetter(Ursa.Controls.BreadcrumbItem.IconProperty, binding);
public static Style<T> Command<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Ursa.Controls.BreadcrumbItem
=> style._addSetter(Ursa.Controls.BreadcrumbItem.CommandProperty, value);
public static Style<T> Command<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.BreadcrumbItem
=> style._addSetter(Ursa.Controls.BreadcrumbItem.CommandProperty, binding);
public static Style<T> IconTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.BreadcrumbItem
=> style._addSetter(Ursa.Controls.BreadcrumbItem.IconTemplateProperty, value);
public static Style<T> IconTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.BreadcrumbItem
=> style._addSetter(Ursa.Controls.BreadcrumbItem.IconTemplateProperty, binding);
public static Style<T> IsReadOnly<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.BreadcrumbItem
=> style._addSetter(Ursa.Controls.BreadcrumbItem.IsReadOnlyProperty, value);
public static Style<T> IsReadOnly<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.BreadcrumbItem
=> style._addSetter(Ursa.Controls.BreadcrumbItem.IsReadOnlyProperty, binding);
}

