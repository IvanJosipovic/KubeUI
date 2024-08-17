using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using InfoBar = FluentAvalonia.UI.Controls.InfoBar;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Avalonia.Markup.Declarative;
public static partial class InfoBarExtensions
{
public static Style<T> IsOpen<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.InfoBar
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.IsOpenProperty, value);
public static Style<T> IsOpen<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.IsOpenProperty, binding);
public static Style<T> Title<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.InfoBar
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.TitleProperty, value);
public static Style<T> Title<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.TitleProperty, binding);
public static Style<T> Message<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.InfoBar
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.MessageProperty, value);
public static Style<T> Message<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.MessageProperty, binding);
public static Style<T> Severity<T>(this Style<T> style, FluentAvalonia.UI.Controls.InfoBarSeverity value) where T : FluentAvalonia.UI.Controls.InfoBar
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.SeverityProperty, value);
public static Style<T> Severity<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.SeverityProperty, binding);
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.InfoBar
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.IconSourceProperty, value);
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.IconSourceProperty, binding);
public static Style<T> IsIconVisible<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.InfoBar
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.IsIconVisibleProperty, value);
public static Style<T> IsIconVisible<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.IsIconVisibleProperty, binding);
public static Style<T> IsClosable<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.InfoBar
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.IsClosableProperty, value);
public static Style<T> IsClosable<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.IsClosableProperty, binding);
public static Style<T> CloseButtonCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : FluentAvalonia.UI.Controls.InfoBar
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandProperty, value);
public static Style<T> CloseButtonCommand<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandProperty, binding);
public static Style<T> CloseButtonCommandParameter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.InfoBar
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandParameterProperty, value);
public static Style<T> CloseButtonCommandParameter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.CloseButtonCommandParameterProperty, binding);
public static Style<T> ActionButton<T>(this Style<T> style, Avalonia.Controls.Control value) where T : FluentAvalonia.UI.Controls.InfoBar
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.ActionButtonProperty, value);
public static Style<T> ActionButton<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.InfoBar
=> style._addSetter(FluentAvalonia.UI.Controls.InfoBar.ActionButtonProperty, binding);
}

