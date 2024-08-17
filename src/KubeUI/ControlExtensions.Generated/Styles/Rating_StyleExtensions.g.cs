using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Rating = Ursa.Controls.Rating;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class RatingExtensions
{
public static Style<T> Value<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.Rating
=> style._addSetter(Ursa.Controls.Rating.ValueProperty, value);
public static Style<T> Value<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Rating
=> style._addSetter(Ursa.Controls.Rating.ValueProperty, binding);
public static Style<T> AllowClear<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Rating
=> style._addSetter(Ursa.Controls.Rating.AllowClearProperty, value);
public static Style<T> AllowClear<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Rating
=> style._addSetter(Ursa.Controls.Rating.AllowClearProperty, binding);
public static Style<T> AllowHalf<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.Rating
=> style._addSetter(Ursa.Controls.Rating.AllowHalfProperty, value);
public static Style<T> AllowHalf<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Rating
=> style._addSetter(Ursa.Controls.Rating.AllowHalfProperty, binding);
public static Style<T> Character<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.Rating
=> style._addSetter(Ursa.Controls.Rating.CharacterProperty, value);
public static Style<T> Character<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Rating
=> style._addSetter(Ursa.Controls.Rating.CharacterProperty, binding);
public static Style<T> Count<T>(this Style<T> style, System.Int32 value) where T : Ursa.Controls.Rating
=> style._addSetter(Ursa.Controls.Rating.CountProperty, value);
public static Style<T> Count<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Rating
=> style._addSetter(Ursa.Controls.Rating.CountProperty, binding);
public static Style<T> DefaultValue<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.Rating
=> style._addSetter(Ursa.Controls.Rating.DefaultValueProperty, value);
public static Style<T> DefaultValue<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Rating
=> style._addSetter(Ursa.Controls.Rating.DefaultValueProperty, binding);
public static Style<T> Size<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.Rating
=> style._addSetter(Ursa.Controls.Rating.SizeProperty, value);
public static Style<T> Size<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Rating
=> style._addSetter(Ursa.Controls.Rating.SizeProperty, binding);
public static Style<T> ItemTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.Rating
=> style._addSetter(Ursa.Controls.Rating.ItemTemplateProperty, value);
public static Style<T> ItemTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.Rating
=> style._addSetter(Ursa.Controls.Rating.ItemTemplateProperty, binding);
}

