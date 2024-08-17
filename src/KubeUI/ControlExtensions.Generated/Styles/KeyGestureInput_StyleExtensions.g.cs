using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using KeyGestureInput = Ursa.Controls.KeyGestureInput;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class KeyGestureInputExtensions
{
public static Style<T> Gesture<T>(this Style<T> style, Avalonia.Input.KeyGesture value) where T : Ursa.Controls.KeyGestureInput
=> style._addSetter(Ursa.Controls.KeyGestureInput.GestureProperty, value);
public static Style<T> Gesture<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.KeyGestureInput
=> style._addSetter(Ursa.Controls.KeyGestureInput.GestureProperty, binding);
public static Style<T> AcceptableKeys<T>(this Style<T> style, System.Collections.Generic.IList<Avalonia.Input.Key> value) where T : Ursa.Controls.KeyGestureInput
=> style._addSetter(Ursa.Controls.KeyGestureInput.AcceptableKeysProperty, value);
public static Style<T> AcceptableKeys<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.KeyGestureInput
=> style._addSetter(Ursa.Controls.KeyGestureInput.AcceptableKeysProperty, binding);
public static Style<T> ConsiderKeyModifiers<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.KeyGestureInput
=> style._addSetter(Ursa.Controls.KeyGestureInput.ConsiderKeyModifiersProperty, value);
public static Style<T> ConsiderKeyModifiers<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.KeyGestureInput
=> style._addSetter(Ursa.Controls.KeyGestureInput.ConsiderKeyModifiersProperty, binding);
public static Style<T> HorizontalContentAlignment<T>(this Style<T> style, Avalonia.Layout.HorizontalAlignment value) where T : Ursa.Controls.KeyGestureInput
=> style._addSetter(Ursa.Controls.KeyGestureInput.HorizontalContentAlignmentProperty, value);
public static Style<T> HorizontalContentAlignment<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.KeyGestureInput
=> style._addSetter(Ursa.Controls.KeyGestureInput.HorizontalContentAlignmentProperty, binding);
public static Style<T> VerticalContentAlignment<T>(this Style<T> style, Avalonia.Layout.VerticalAlignment value) where T : Ursa.Controls.KeyGestureInput
=> style._addSetter(Ursa.Controls.KeyGestureInput.VerticalContentAlignmentProperty, value);
public static Style<T> VerticalContentAlignment<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.KeyGestureInput
=> style._addSetter(Ursa.Controls.KeyGestureInput.VerticalContentAlignmentProperty, binding);
public static Style<T> InnerLeftContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.KeyGestureInput
=> style._addSetter(Ursa.Controls.KeyGestureInput.InnerLeftContentProperty, value);
public static Style<T> InnerLeftContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.KeyGestureInput
=> style._addSetter(Ursa.Controls.KeyGestureInput.InnerLeftContentProperty, binding);
public static Style<T> InnerRightContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.KeyGestureInput
=> style._addSetter(Ursa.Controls.KeyGestureInput.InnerRightContentProperty, value);
public static Style<T> InnerRightContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.KeyGestureInput
=> style._addSetter(Ursa.Controls.KeyGestureInput.InnerRightContentProperty, binding);
}

