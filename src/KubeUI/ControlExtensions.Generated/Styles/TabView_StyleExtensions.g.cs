using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TabView = FluentAvalonia.UI.Controls.TabView;

namespace Avalonia.Markup.Declarative;
public static partial class TabViewExtensions
{
public static Style<T> TabWidthMode<T>(this Style<T> style, FluentAvalonia.UI.Controls.TabViewWidthMode value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabWidthModeProperty, value);
public static Style<T> TabWidthMode<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabWidthModeProperty, binding);
public static Style<T> CloseButtonOverlayMode<T>(this Style<T> style, FluentAvalonia.UI.Controls.TabViewCloseButtonOverlayMode value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.CloseButtonOverlayModeProperty, value);
public static Style<T> CloseButtonOverlayMode<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.CloseButtonOverlayModeProperty, binding);
public static Style<T> TabStripHeader<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabStripHeaderProperty, value);
public static Style<T> TabStripHeader<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabStripHeaderProperty, binding);
public static Style<T> TabStripHeaderTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabStripHeaderTemplateProperty, value);
public static Style<T> TabStripHeaderTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabStripHeaderTemplateProperty, binding);
public static Style<T> TabStripFooter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabStripFooterProperty, value);
public static Style<T> TabStripFooter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabStripFooterProperty, binding);
public static Style<T> TabStripFooterTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabStripFooterTemplateProperty, value);
public static Style<T> TabStripFooterTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabStripFooterTemplateProperty, binding);
public static Style<T> IsAddTabButtonVisible<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.IsAddTabButtonVisibleProperty, value);
public static Style<T> IsAddTabButtonVisible<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.IsAddTabButtonVisibleProperty, binding);
public static Style<T> AddTabButtonCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandProperty, value);
public static Style<T> AddTabButtonCommand<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandProperty, binding);
public static Style<T> AddTabButtonCommandParameter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandParameterProperty, value);
public static Style<T> AddTabButtonCommandParameter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.AddTabButtonCommandParameterProperty, binding);
public static Style<T> TabItemTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabItemTemplateProperty, value);
public static Style<T> TabItemTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.TabItemTemplateProperty, binding);
public static Style<T> CanDragTabs<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.CanDragTabsProperty, value);
public static Style<T> CanDragTabs<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.CanDragTabsProperty, binding);
public static Style<T> CanReorderTabs<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.CanReorderTabsProperty, value);
public static Style<T> CanReorderTabs<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.CanReorderTabsProperty, binding);
public static Style<T> AllowDropTabs<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.AllowDropTabsProperty, value);
public static Style<T> AllowDropTabs<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TabView
=> style._addSetter(FluentAvalonia.UI.Controls.TabView.AllowDropTabsProperty, binding);
}

