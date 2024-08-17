using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using NavigationView = FluentAvalonia.UI.Controls.NavigationView;
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class NavigationViewExtensions
{
public static Style<T> AlwaysShowHeader<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.AlwaysShowHeaderProperty, value);
public static Style<T> AlwaysShowHeader<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.AlwaysShowHeaderProperty, binding);
public static Style<T> AutoCompleteBox<T>(this Style<T> style, Avalonia.Controls.AutoCompleteBox value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.AutoCompleteBoxProperty, value);
public static Style<T> AutoCompleteBox<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.AutoCompleteBoxProperty, binding);
public static Style<T> CompactModeThresholdWidth<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.CompactModeThresholdWidthProperty, value);
public static Style<T> CompactModeThresholdWidth<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.CompactModeThresholdWidthProperty, binding);
public static Style<T> CompactPaneLength<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.CompactPaneLengthProperty, value);
public static Style<T> CompactPaneLength<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.CompactPaneLengthProperty, binding);
public static Style<T> ContentOverlay<T>(this Style<T> style, Avalonia.Controls.Control value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.ContentOverlayProperty, value);
public static Style<T> ContentOverlay<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.ContentOverlayProperty, binding);
public static Style<T> ExpandedModeThresholdWidth<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.ExpandedModeThresholdWidthProperty, value);
public static Style<T> ExpandedModeThresholdWidth<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.ExpandedModeThresholdWidthProperty, binding);
public static Style<T> FooterMenuItemsSource<T>(this Style<T> style, System.Collections.IEnumerable value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.FooterMenuItemsSourceProperty, value);
public static Style<T> FooterMenuItemsSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.FooterMenuItemsSourceProperty, binding);
public static Style<T> IsBackButtonVisible<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsBackButtonVisibleProperty, value);
public static Style<T> IsBackButtonVisible<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsBackButtonVisibleProperty, binding);
public static Style<T> IsBackEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsBackEnabledProperty, value);
public static Style<T> IsBackEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsBackEnabledProperty, binding);
public static Style<T> IsPaneOpen<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsPaneOpenProperty, value);
public static Style<T> IsPaneOpen<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsPaneOpenProperty, binding);
public static Style<T> IsPaneToggleButtonVisible<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsPaneToggleButtonVisibleProperty, value);
public static Style<T> IsPaneToggleButtonVisible<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsPaneToggleButtonVisibleProperty, binding);
public static Style<T> IsPaneVisible<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsPaneVisibleProperty, value);
public static Style<T> IsPaneVisible<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsPaneVisibleProperty, binding);
public static Style<T> IsSettingsVisible<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsSettingsVisibleProperty, value);
public static Style<T> IsSettingsVisible<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.IsSettingsVisibleProperty, binding);
public static Style<T> MenuItemsSource<T>(this Style<T> style, System.Collections.IEnumerable value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.MenuItemsSourceProperty, value);
public static Style<T> MenuItemsSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.MenuItemsSourceProperty, binding);
public static Style<T> MenuItemTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateProperty, value);
public static Style<T> MenuItemTemplate<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateProperty, binding);
public static Style<T> MenuItemTemplateSelector<T>(this Style<T> style, FluentAvalonia.UI.Controls.DataTemplateSelector value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateSelectorProperty, value);
public static Style<T> MenuItemTemplateSelector<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.MenuItemTemplateSelectorProperty, binding);
public static Style<T> OpenPaneLength<T>(this Style<T> style, System.Double value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.OpenPaneLengthProperty, value);
public static Style<T> OpenPaneLength<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.OpenPaneLengthProperty, binding);
public static Style<T> PaneCustomContent<T>(this Style<T> style, Avalonia.Controls.Control value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.PaneCustomContentProperty, value);
public static Style<T> PaneCustomContent<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.PaneCustomContentProperty, binding);
public static Style<T> PaneDisplayMode<T>(this Style<T> style, FluentAvalonia.UI.Controls.NavigationViewPaneDisplayMode value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.PaneDisplayModeProperty, value);
public static Style<T> PaneDisplayMode<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.PaneDisplayModeProperty, binding);
public static Style<T> PaneFooter<T>(this Style<T> style, Avalonia.Controls.Control value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.PaneFooterProperty, value);
public static Style<T> PaneFooter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.PaneFooterProperty, binding);
public static Style<T> PaneHeader<T>(this Style<T> style, Avalonia.Controls.Control value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.PaneHeaderProperty, value);
public static Style<T> PaneHeader<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.PaneHeaderProperty, binding);
public static Style<T> PaneTitle<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.PaneTitleProperty, value);
public static Style<T> PaneTitle<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.PaneTitleProperty, binding);
public static Style<T> SelectionFollowsFocus<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.SelectionFollowsFocusProperty, value);
public static Style<T> SelectionFollowsFocus<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.NavigationView
=> style._addSetter(FluentAvalonia.UI.Controls.NavigationView.SelectionFollowsFocusProperty, binding);
}

