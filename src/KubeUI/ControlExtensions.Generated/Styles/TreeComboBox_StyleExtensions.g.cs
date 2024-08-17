using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TreeComboBox = Ursa.Controls.TreeComboBox;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TreeComboBoxExtensions
{
public static Style<T> MaxDropDownHeight<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.TreeComboBox
=> style._addSetter(Ursa.Controls.TreeComboBox.MaxDropDownHeightProperty, value);
public static Style<T> MaxDropDownHeight<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TreeComboBox
=> style._addSetter(Ursa.Controls.TreeComboBox.MaxDropDownHeightProperty, binding);
public static Style<T> Watermark<T>(this Style<T> style, System.String value) where T : Ursa.Controls.TreeComboBox
=> style._addSetter(Ursa.Controls.TreeComboBox.WatermarkProperty, value);
public static Style<T> Watermark<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TreeComboBox
=> style._addSetter(Ursa.Controls.TreeComboBox.WatermarkProperty, binding);
public static Style<T> IsDropDownOpen<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.TreeComboBox
=> style._addSetter(Ursa.Controls.TreeComboBox.IsDropDownOpenProperty, value);
public static Style<T> IsDropDownOpen<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TreeComboBox
=> style._addSetter(Ursa.Controls.TreeComboBox.IsDropDownOpenProperty, binding);
public static Style<T> HorizontalContentAlignment<T>(this Style<T> style, Avalonia.Layout.HorizontalAlignment value) where T : Ursa.Controls.TreeComboBox
=> style._addSetter(Ursa.Controls.TreeComboBox.HorizontalContentAlignmentProperty, value);
public static Style<T> HorizontalContentAlignment<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TreeComboBox
=> style._addSetter(Ursa.Controls.TreeComboBox.HorizontalContentAlignmentProperty, binding);
public static Style<T> VerticalContentAlignment<T>(this Style<T> style, Avalonia.Layout.VerticalAlignment value) where T : Ursa.Controls.TreeComboBox
=> style._addSetter(Ursa.Controls.TreeComboBox.VerticalContentAlignmentProperty, value);
public static Style<T> VerticalContentAlignment<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TreeComboBox
=> style._addSetter(Ursa.Controls.TreeComboBox.VerticalContentAlignmentProperty, binding);
public static Style<T> SelectedItemTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.TreeComboBox
=> style._addSetter(Ursa.Controls.TreeComboBox.SelectedItemTemplateProperty, value);
public static Style<T> SelectedItemTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TreeComboBox
=> style._addSetter(Ursa.Controls.TreeComboBox.SelectedItemTemplateProperty, binding);
public static Style<T> InnerLeftContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TreeComboBox
=> style._addSetter(Ursa.Controls.TreeComboBox.InnerLeftContentProperty, value);
public static Style<T> InnerLeftContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TreeComboBox
=> style._addSetter(Ursa.Controls.TreeComboBox.InnerLeftContentProperty, binding);
public static Style<T> InnerRightContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TreeComboBox
=> style._addSetter(Ursa.Controls.TreeComboBox.InnerRightContentProperty, value);
public static Style<T> InnerRightContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TreeComboBox
=> style._addSetter(Ursa.Controls.TreeComboBox.InnerRightContentProperty, binding);
public static Style<T> PopupInnerTopContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TreeComboBox
=> style._addSetter(Ursa.Controls.TreeComboBox.PopupInnerTopContentProperty, value);
public static Style<T> PopupInnerTopContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TreeComboBox
=> style._addSetter(Ursa.Controls.TreeComboBox.PopupInnerTopContentProperty, binding);
public static Style<T> PopupInnerBottomContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TreeComboBox
=> style._addSetter(Ursa.Controls.TreeComboBox.PopupInnerBottomContentProperty, value);
public static Style<T> PopupInnerBottomContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TreeComboBox
=> style._addSetter(Ursa.Controls.TreeComboBox.PopupInnerBottomContentProperty, binding);
}

