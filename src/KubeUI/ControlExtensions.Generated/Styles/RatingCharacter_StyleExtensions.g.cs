using Avalonia.Data;
using Avalonia.Data.Converters;
using RatingCharacter = Ursa.Controls.RatingCharacter;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class RatingCharacterExtensions
{
public static Style<T> AllowHalf<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.RatingCharacter
=> style._addSetter(Ursa.Controls.RatingCharacter.AllowHalfProperty, value);
public static Style<T> AllowHalf<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RatingCharacter
=> style._addSetter(Ursa.Controls.RatingCharacter.AllowHalfProperty, binding);
public static Style<T> Character<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.RatingCharacter
=> style._addSetter(Ursa.Controls.RatingCharacter.CharacterProperty, value);
public static Style<T> Character<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RatingCharacter
=> style._addSetter(Ursa.Controls.RatingCharacter.CharacterProperty, binding);
public static Style<T> Size<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.RatingCharacter
=> style._addSetter(Ursa.Controls.RatingCharacter.SizeProperty, value);
public static Style<T> Size<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.RatingCharacter
=> style._addSetter(Ursa.Controls.RatingCharacter.SizeProperty, binding);
}

