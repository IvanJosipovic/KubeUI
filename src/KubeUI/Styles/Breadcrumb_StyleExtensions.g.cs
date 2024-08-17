using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Breadcrumb = Ursa.Controls.Breadcrumb;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class BreadcrumbExtensions
{
public static Style<T> IconBinding<T>(this Style<T> style, Avalonia.Data.IBinding value) where T : Ursa.Controls.Breadcrumb
=> style._addSetter(Ursa.Controls.Breadcrumb.IconBindingProperty, value);
//Skipped IconBinding because already exist in value setters
public static Style<T> CommandBinding<T>(this Style<T> style, Avalonia.Data.IBinding value) where T : Ursa.Controls.Breadcrumb
=> style._addSetter(Ursa.Controls.Breadcrumb.CommandBindingProperty, value);
//Skipped CommandBinding because already exist in value setters
public static Style<T> Separator<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.Breadcrumb
=> style._addSetter(Ursa.Controls.Breadcrumb.SeparatorProperty, value);
public static Style<T> Separator<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Breadcrumb
=> style._addSetter(Ursa.Controls.Breadcrumb.SeparatorProperty, binding);
public static Style<T> IconTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.Breadcrumb
=> style._addSetter(Ursa.Controls.Breadcrumb.IconTemplateProperty, value);
public static Style<T> IconTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Breadcrumb
=> style._addSetter(Ursa.Controls.Breadcrumb.IconTemplateProperty, binding);
}

