using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using NumberBox = FluentAvalonia.UI.Controls.NumberBox;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class NumberBoxExtensions
{
public static Style<T> AcceptsExpression<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.AcceptsExpressionProperty, value);
public static Style<T> AcceptsExpression<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.AcceptsExpressionProperty, binding);
public static Style<T> Description<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.DescriptionProperty, value);
public static Style<T> Description<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.DescriptionProperty, binding);
public static Style<T> Header<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.HeaderProperty, value);
public static Style<T> Header<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.HeaderProperty, binding);
public static Style<T> HeaderTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.HeaderTemplateProperty, value);
public static Style<T> HeaderTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.HeaderTemplateProperty, binding);
public static Style<T> IsWrapEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.IsWrapEnabledProperty, value);
public static Style<T> IsWrapEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.IsWrapEnabledProperty, binding);
public static Style<T> LargeChange<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.LargeChangeProperty, value);
public static Style<T> LargeChange<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.LargeChangeProperty, binding);
public static Style<T> Minimum<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.MinimumProperty, value);
public static Style<T> Minimum<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.MinimumProperty, binding);
public static Style<T> Maximum<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.MaximumProperty, value);
public static Style<T> Maximum<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.MaximumProperty, binding);
public static Style<T> PlaceholderText<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.PlaceholderTextProperty, value);
public static Style<T> PlaceholderText<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.PlaceholderTextProperty, binding);
public static Style<T> SelectionFlyout<T>(this Style<T> style, Avalonia.Controls.Primitives.FlyoutBase value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.SelectionFlyoutProperty, value);
public static Style<T> SelectionFlyout<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.SelectionFlyoutProperty, binding);
public static Style<T> SelectionHighlightColor<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.SelectionHighlightColorProperty, value);
public static Style<T> SelectionHighlightColor<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.SelectionHighlightColorProperty, binding);
public static Style<T> SmallChange<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.SmallChangeProperty, value);
public static Style<T> SmallChange<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.SmallChangeProperty, binding);
public static Style<T> SpinButtonPlacementMode<T>(this Style<T> style, FluentAvalonia.UI.Controls.NumberBoxSpinButtonPlacementMode value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.SpinButtonPlacementModeProperty, value);
public static Style<T> SpinButtonPlacementMode<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.SpinButtonPlacementModeProperty, binding);
public static Style<T> TextReadingOrder<T>(this Style<T> style, FluentAvalonia.UI.Controls.TextReadingOrder value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.TextReadingOrderProperty, value);
public static Style<T> TextReadingOrder<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.TextReadingOrderProperty, binding);
public static Style<T> ValidationMode<T>(this Style<T> style, FluentAvalonia.UI.Controls.NumberBoxValidationMode value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.ValidationModeProperty, value);
public static Style<T> ValidationMode<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.ValidationModeProperty, binding);
public static Style<T> Value<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.ValueProperty, value);
public static Style<T> Value<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.ValueProperty, binding);
public static Style<T> TextAlignment<T>(this Style<T> style, Avalonia.Media.TextAlignment value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.TextAlignmentProperty, value);
public static Style<T> TextAlignment<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.TextAlignmentProperty, binding);
public static Style<T> SimpleNumberFormat<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.SimpleNumberFormatProperty, value);
public static Style<T> SimpleNumberFormat<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NumberBox
=> style._addSetter(FluentAvalonia.UI.Controls.NumberBox.SimpleNumberFormatProperty, binding);
}

