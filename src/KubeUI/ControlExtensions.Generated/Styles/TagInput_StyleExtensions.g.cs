using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TagInput = Ursa.Controls.TagInput;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class TagInputExtensions
{
public static Style<T> Tags<T>(this Style<T> style, System.Collections.Generic.IList<System.String> value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.TagsProperty, value);
public static Style<T> Tags<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.TagsProperty, binding);
public static Style<T> Watermark<T>(this Style<T> style, System.String value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.WatermarkProperty, value);
public static Style<T> Watermark<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.WatermarkProperty, binding);
public static Style<T> AcceptsReturn<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.AcceptsReturnProperty, value);
public static Style<T> AcceptsReturn<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.AcceptsReturnProperty, binding);
public static Style<T> MaxCount<T>(this Style<T> style, System.Int32 value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.MaxCountProperty, value);
public static Style<T> MaxCount<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.MaxCountProperty, binding);
public static Style<T> InputTheme<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.InputThemeProperty, value);
public static Style<T> InputTheme<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.InputThemeProperty, binding);
public static Style<T> ItemTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.ItemTemplateProperty, value);
public static Style<T> ItemTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.ItemTemplateProperty, binding);
public static Style<T> Separator<T>(this Style<T> style, System.String value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.SeparatorProperty, value);
public static Style<T> Separator<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.SeparatorProperty, binding);
public static Style<T> LostFocusBehavior<T>(this Style<T> style, Ursa.Controls.LostFocusBehavior value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.LostFocusBehaviorProperty, value);
public static Style<T> LostFocusBehavior<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.LostFocusBehaviorProperty, binding);
public static Style<T> AllowDuplicates<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.AllowDuplicatesProperty, value);
public static Style<T> AllowDuplicates<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.AllowDuplicatesProperty, binding);
public static Style<T> InnerLeftContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.InnerLeftContentProperty, value);
public static Style<T> InnerLeftContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.InnerLeftContentProperty, binding);
public static Style<T> InnerRightContent<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.InnerRightContentProperty, value);
public static Style<T> InnerRightContent<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.TagInput
=> style._addSetter(Ursa.Controls.TagInput.InnerRightContentProperty, binding);
}

