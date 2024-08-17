using Avalonia.Data;
using Avalonia.Data.Converters;
using ElasticWrapPanel = Ursa.Controls.ElasticWrapPanel;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class ElasticWrapPanelExtensions
{
public static Style<T> IsFillHorizontal<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.ElasticWrapPanel
=> style._addSetter(Ursa.Controls.ElasticWrapPanel.IsFillHorizontalProperty, value);
public static Style<T> IsFillHorizontal<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ElasticWrapPanel
=> style._addSetter(Ursa.Controls.ElasticWrapPanel.IsFillHorizontalProperty, binding);
public static Style<T> IsFillVertical<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.ElasticWrapPanel
=> style._addSetter(Ursa.Controls.ElasticWrapPanel.IsFillVerticalProperty, value);
public static Style<T> IsFillVertical<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.ElasticWrapPanel
=> style._addSetter(Ursa.Controls.ElasticWrapPanel.IsFillVerticalProperty, binding);
}

