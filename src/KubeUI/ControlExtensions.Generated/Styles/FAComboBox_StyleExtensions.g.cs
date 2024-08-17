using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using FAComboBox = FluentAvalonia.UI.Controls.FAComboBox;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class FAComboBoxExtensions
{
public static Style<T> MaxDropDownHeight<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.MaxDropDownHeightProperty, value);
public static Style<T> MaxDropDownHeight<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.MaxDropDownHeightProperty, binding);
public static Style<T> IsEditable<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.IsEditableProperty, value);
public static Style<T> IsEditable<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.IsEditableProperty, binding);
public static Style<T> IsDropDownOpen<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.IsDropDownOpenProperty, value);
public static Style<T> IsDropDownOpen<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.IsDropDownOpenProperty, binding);
public static Style<T> PlaceholderText<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderTextProperty, value);
public static Style<T> PlaceholderText<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderTextProperty, binding);
public static Style<T> SelectionChangedTrigger<T>(this Style<T> style, FluentAvalonia.UI.Controls.FAComboBoxSelectionChangedTrigger value) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.SelectionChangedTriggerProperty, value);
public static Style<T> SelectionChangedTrigger<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.SelectionChangedTriggerProperty, binding);
public static Style<T> PlaceholderForeground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderForegroundProperty, value);
public static Style<T> PlaceholderForeground<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.PlaceholderForegroundProperty, binding);
public static Style<T> TextBoxTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.TextBoxThemeProperty, value);
public static Style<T> TextBoxTheme<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.TextBoxThemeProperty, binding);
public static Style<T> Text<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.TextProperty, value);
public static Style<T> Text<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.TextProperty, binding);
public static Style<T> HorizontalContentAlignment<T>(this Style<T> style, Avalonia.Layout.HorizontalAlignment value) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.HorizontalContentAlignmentProperty, value);
public static Style<T> HorizontalContentAlignment<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.HorizontalContentAlignmentProperty, binding);
public static Style<T> VerticalContentAlignment<T>(this Style<T> style, Avalonia.Layout.VerticalAlignment value) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.VerticalContentAlignmentProperty, value);
public static Style<T> VerticalContentAlignment<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.FAComboBox
=> style._addSetter(FluentAvalonia.UI.Controls.FAComboBox.VerticalContentAlignmentProperty, binding);
}

