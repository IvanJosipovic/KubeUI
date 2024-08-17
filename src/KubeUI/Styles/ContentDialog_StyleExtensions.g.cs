using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using ContentDialog = FluentAvalonia.UI.Controls.ContentDialog;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Avalonia.Markup.Declarative;
public static partial class ContentDialogExtensions
{
public static Style<T> CloseButtonCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandProperty, value);
public static Style<T> CloseButtonCommand<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandProperty, binding);
public static Style<T> CloseButtonCommandParameter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandParameterProperty, value);
public static Style<T> CloseButtonCommandParameter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonCommandParameterProperty, binding);
public static Style<T> CloseButtonText<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonTextProperty, value);
public static Style<T> CloseButtonText<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.CloseButtonTextProperty, binding);
public static Style<T> DefaultButton<T>(this Style<T> style, FluentAvalonia.UI.Controls.ContentDialogButton value) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.DefaultButtonProperty, value);
public static Style<T> DefaultButton<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.DefaultButtonProperty, binding);
public static Style<T> IsPrimaryButtonEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.IsPrimaryButtonEnabledProperty, value);
public static Style<T> IsPrimaryButtonEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.IsPrimaryButtonEnabledProperty, binding);
public static Style<T> IsSecondaryButtonEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.IsSecondaryButtonEnabledProperty, value);
public static Style<T> IsSecondaryButtonEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.IsSecondaryButtonEnabledProperty, binding);
public static Style<T> PrimaryButtonCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandProperty, value);
public static Style<T> PrimaryButtonCommand<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandProperty, binding);
public static Style<T> PrimaryButtonCommandParameter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandParameterProperty, value);
public static Style<T> PrimaryButtonCommandParameter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonCommandParameterProperty, binding);
public static Style<T> PrimaryButtonText<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonTextProperty, value);
public static Style<T> PrimaryButtonText<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.PrimaryButtonTextProperty, binding);
public static Style<T> SecondaryButtonCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandProperty, value);
public static Style<T> SecondaryButtonCommand<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandProperty, binding);
public static Style<T> SecondaryButtonCommandParameter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandParameterProperty, value);
public static Style<T> SecondaryButtonCommandParameter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonCommandParameterProperty, binding);
public static Style<T> SecondaryButtonText<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonTextProperty, value);
public static Style<T> SecondaryButtonText<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.SecondaryButtonTextProperty, binding);
public static Style<T> Title<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.TitleProperty, value);
public static Style<T> Title<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.TitleProperty, binding);
public static Style<T> TitleTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.TitleTemplateProperty, value);
public static Style<T> TitleTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.TitleTemplateProperty, binding);
public static Style<T> FullSizeDesired<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.FullSizeDesiredProperty, value);
public static Style<T> FullSizeDesired<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.ContentDialog
=> style._addSetter(FluentAvalonia.UI.Controls.ContentDialog.FullSizeDesiredProperty, binding);
}

