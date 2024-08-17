using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using ThemeSelectorBase = Ursa.Controls.ThemeSelectorBase;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ThemeSelectorBaseExtensions
{
public static Style<T> SelectedTheme<T>(this Style<T> style, Avalonia.Styling.ThemeVariant value) where T : Ursa.Controls.ThemeSelectorBase
=> style._addSetter(Ursa.Controls.ThemeSelectorBase.SelectedThemeProperty, value);
public static Style<T> SelectedTheme<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ThemeSelectorBase
=> style._addSetter(Ursa.Controls.ThemeSelectorBase.SelectedThemeProperty, binding);
public static Style<T> Mode<T>(this Style<T> style, Ursa.Controls.ThemeSelectorMode value) where T : Ursa.Controls.ThemeSelectorBase
=> style._addSetter(Ursa.Controls.ThemeSelectorBase.ModeProperty, value);
public static Style<T> Mode<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ThemeSelectorBase
=> style._addSetter(Ursa.Controls.ThemeSelectorBase.ModeProperty, binding);
public static Style<T> TargetScope<T>(this Style<T> style, Avalonia.Controls.ThemeVariantScope value) where T : Ursa.Controls.ThemeSelectorBase
=> style._addSetter(Ursa.Controls.ThemeSelectorBase.TargetScopeProperty, value);
public static Style<T> TargetScope<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ThemeSelectorBase
=> style._addSetter(Ursa.Controls.ThemeSelectorBase.TargetScopeProperty, binding);
}

