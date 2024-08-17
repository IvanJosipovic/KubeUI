using Avalonia.Controls.Notifications;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Banner = Ursa.Controls.Banner;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class BannerExtensions
{
public static Style<T> CanClose<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Banner
=> style._addSetter(Ursa.Controls.Banner.CanCloseProperty, value);
public static Style<T> CanClose<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Banner
=> style._addSetter(Ursa.Controls.Banner.CanCloseProperty, binding);
public static Style<T> ShowIcon<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Banner
=> style._addSetter(Ursa.Controls.Banner.ShowIconProperty, value);
public static Style<T> ShowIcon<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Banner
=> style._addSetter(Ursa.Controls.Banner.ShowIconProperty, binding);
public static Style<T> Icon<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.Banner
=> style._addSetter(Ursa.Controls.Banner.IconProperty, value);
public static Style<T> Icon<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Banner
=> style._addSetter(Ursa.Controls.Banner.IconProperty, binding);
public static Style<T> Type<T>(this Style<T> style, Avalonia.Controls.Notifications.NotificationType value) where T : Ursa.Controls.Banner
=> style._addSetter(Ursa.Controls.Banner.TypeProperty, value);
public static Style<T> Type<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Banner
=> style._addSetter(Ursa.Controls.Banner.TypeProperty, binding);
}

