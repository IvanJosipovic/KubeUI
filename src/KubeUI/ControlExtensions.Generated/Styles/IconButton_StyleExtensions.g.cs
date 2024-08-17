using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using IconButton = Ursa.Controls.IconButton;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Common;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class IconButtonExtensions
{
public static Style<T> Icon<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.IconButton
=> style._addSetter(Ursa.Controls.IconButton.IconProperty, value);
public static Style<T> Icon<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.IconButton
=> style._addSetter(Ursa.Controls.IconButton.IconProperty, binding);
public static Style<T> IconTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.IconButton
=> style._addSetter(Ursa.Controls.IconButton.IconTemplateProperty, value);
public static Style<T> IconTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.IconButton
=> style._addSetter(Ursa.Controls.IconButton.IconTemplateProperty, binding);
public static Style<T> IsLoading<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.IconButton
=> style._addSetter(Ursa.Controls.IconButton.IsLoadingProperty, value);
public static Style<T> IsLoading<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.IconButton
=> style._addSetter(Ursa.Controls.IconButton.IsLoadingProperty, binding);
public static Style<T> IconPlacement<T>(this Style<T> style, Ursa.Common.Position value) where T : Ursa.Controls.IconButton
=> style._addSetter(Ursa.Controls.IconButton.IconPlacementProperty, value);
public static Style<T> IconPlacement<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.IconButton
=> style._addSetter(Ursa.Controls.IconButton.IconPlacementProperty, binding);
}

