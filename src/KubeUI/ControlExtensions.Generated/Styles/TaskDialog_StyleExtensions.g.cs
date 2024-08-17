using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TaskDialog = FluentAvalonia.UI.Controls.TaskDialog;

namespace Avalonia.Markup.Declarative;
public static partial class TaskDialogExtensions
{
public static Style<T> Title<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.TitleProperty, value);
public static Style<T> Title<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.TitleProperty, binding);
public static Style<T> Header<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.HeaderProperty, value);
public static Style<T> Header<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.HeaderProperty, binding);
public static Style<T> SubHeader<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.SubHeaderProperty, value);
public static Style<T> SubHeader<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.SubHeaderProperty, binding);
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.IconSourceProperty, value);
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.IconSourceProperty, binding);
public static Style<T> FooterVisibility<T>(this Style<T> style, FluentAvalonia.UI.Controls.TaskDialogFooterVisibility value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.FooterVisibilityProperty, value);
public static Style<T> FooterVisibility<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.FooterVisibilityProperty, binding);
public static Style<T> IsFooterExpanded<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.IsFooterExpandedProperty, value);
public static Style<T> IsFooterExpanded<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.IsFooterExpandedProperty, binding);
public static Style<T> Footer<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.FooterProperty, value);
public static Style<T> Footer<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.FooterProperty, binding);
public static Style<T> FooterTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.FooterTemplateProperty, value);
public static Style<T> FooterTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.FooterTemplateProperty, binding);
public static Style<T> ShowProgressBar<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.ShowProgressBarProperty, value);
public static Style<T> ShowProgressBar<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.ShowProgressBarProperty, binding);
public static Style<T> HeaderBackground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.HeaderBackgroundProperty, value);
public static Style<T> HeaderBackground<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.HeaderBackgroundProperty, binding);
public static Style<T> HeaderForeground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.HeaderForegroundProperty, value);
public static Style<T> HeaderForeground<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.HeaderForegroundProperty, binding);
public static Style<T> IconForeground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.IconForegroundProperty, value);
public static Style<T> IconForeground<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TaskDialog
=> style._addSetter(FluentAvalonia.UI.Controls.TaskDialog.IconForegroundProperty, binding);
}

