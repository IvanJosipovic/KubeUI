using Avalonia.Data;
using Avalonia.Data.Converters;
using Skeleton = Ursa.Controls.Skeleton;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class SkeletonExtensions
{
public static Style<T> IsActive<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Skeleton
=> style._addSetter(Ursa.Controls.Skeleton.IsActiveProperty, value);
public static Style<T> IsActive<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Skeleton
=> style._addSetter(Ursa.Controls.Skeleton.IsActiveProperty, binding);
public static Style<T> IsLoading<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Skeleton
=> style._addSetter(Ursa.Controls.Skeleton.IsLoadingProperty, value);
public static Style<T> IsLoading<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Skeleton
=> style._addSetter(Ursa.Controls.Skeleton.IsLoadingProperty, binding);
}

