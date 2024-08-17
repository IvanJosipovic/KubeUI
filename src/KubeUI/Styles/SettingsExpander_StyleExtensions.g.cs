using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using SettingsExpander = FluentAvalonia.UI.Controls.SettingsExpander;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Avalonia.Markup.Declarative;
public static partial class SettingsExpanderExtensions
{
public static Style<T> Description<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.SettingsExpander
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpander.DescriptionProperty, value);
public static Style<T> Description<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpander
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpander.DescriptionProperty, binding);
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.SettingsExpander
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpander.IconSourceProperty, value);
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpander
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpander.IconSourceProperty, binding);
public static Style<T> Footer<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.SettingsExpander
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpander.FooterProperty, value);
public static Style<T> Footer<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpander
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpander.FooterProperty, binding);
public static Style<T> FooterTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.SettingsExpander
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpander.FooterTemplateProperty, value);
public static Style<T> FooterTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpander
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpander.FooterTemplateProperty, binding);
public static Style<T> IsExpanded<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.SettingsExpander
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpander.IsExpandedProperty, value);
public static Style<T> IsExpanded<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpander
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpander.IsExpandedProperty, binding);
public static Style<T> ActionIconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.SettingsExpander
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpander.ActionIconSourceProperty, value);
public static Style<T> ActionIconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpander
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpander.ActionIconSourceProperty, binding);
public static Style<T> IsClickEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.SettingsExpander
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpander.IsClickEnabledProperty, value);
public static Style<T> IsClickEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpander
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpander.IsClickEnabledProperty, binding);
public static Style<T> Command<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : FluentAvalonia.UI.Controls.SettingsExpander
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpander.CommandProperty, value);
public static Style<T> Command<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpander
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpander.CommandProperty, binding);
public static Style<T> CommandParameter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.SettingsExpander
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpander.CommandParameterProperty, value);
public static Style<T> CommandParameter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpander
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpander.CommandParameterProperty, binding);
}

