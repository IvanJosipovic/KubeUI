using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using MultiComboBox = Ursa.Controls.MultiComboBox;
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class MultiComboBoxExtensions
{
public static Style<T> IsDropDownOpen<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.IsDropDownOpenProperty, value);
public static Style<T> IsDropDownOpen<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.IsDropDownOpenProperty, binding);
public static Style<T> MaxDropdownHeight<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.MaxDropdownHeightProperty, value);
public static Style<T> MaxDropdownHeight<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.MaxDropdownHeightProperty, binding);
public static Style<T> MaxSelectionBoxHeight<T>(this Style<T> style, System.Double value) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.MaxSelectionBoxHeightProperty, value);
public static Style<T> MaxSelectionBoxHeight<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.MaxSelectionBoxHeightProperty, binding);
public static Style<T> SelectedItems<T>(this Style<T> style, System.Collections.IList value) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.SelectedItemsProperty, value);
public static Style<T> SelectedItems<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.SelectedItemsProperty, binding);
public static Style<T> InnerLeftContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.InnerLeftContentProperty, value);
public static Style<T> InnerLeftContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.InnerLeftContentProperty, binding);
public static Style<T> InnerRightContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.InnerRightContentProperty, value);
public static Style<T> InnerRightContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.InnerRightContentProperty, binding);
public static Style<T> SelectedItemTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.SelectedItemTemplateProperty, value);
public static Style<T> SelectedItemTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.MultiComboBox
=> style._addSetter(Ursa.Controls.MultiComboBox.SelectedItemTemplateProperty, binding);
}

