using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using SettingsExpanderItem = FluentAvalonia.UI.Controls.SettingsExpanderItem;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Avalonia.Markup.Declarative;
public static partial class SettingsExpanderItemExtensions
{
public static Style<T> Description<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.DescriptionProperty, value);
public static Style<T> Description<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.DescriptionProperty, binding);
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.IconSourceProperty, value);
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.IconSourceProperty, binding);
public static Style<T> Footer<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterProperty, value);
public static Style<T> Footer<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterProperty, binding);
public static Style<T> FooterTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterTemplateProperty, value);
public static Style<T> FooterTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.FooterTemplateProperty, binding);
public static Style<T> ActionIconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.ActionIconSourceProperty, value);
public static Style<T> ActionIconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.ActionIconSourceProperty, binding);
public static Style<T> IsClickEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.IsClickEnabledProperty, value);
public static Style<T> IsClickEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.IsClickEnabledProperty, binding);
public static Style<T> Command<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandProperty, value);
public static Style<T> Command<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandProperty, binding);
public static Style<T> CommandParameter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandParameterProperty, value);
public static Style<T> CommandParameter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.SettingsExpanderItem
=> style._addSetter(FluentAvalonia.UI.Controls.SettingsExpanderItem.CommandParameterProperty, binding);
}

