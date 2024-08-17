using Avalonia.Data;
using Avalonia.Data.Converters;
using Loading = Ursa.Controls.Loading;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class LoadingExtensions
{
public static Style<T> Indicator<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.Loading
=> style._addSetter(Ursa.Controls.Loading.IndicatorProperty, value);
public static Style<T> Indicator<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Loading
=> style._addSetter(Ursa.Controls.Loading.IndicatorProperty, binding);
public static Style<T> IsLoading<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.Loading
=> style._addSetter(Ursa.Controls.Loading.IsLoadingProperty, value);
public static Style<T> IsLoading<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Loading
=> style._addSetter(Ursa.Controls.Loading.IsLoadingProperty, binding);
}

