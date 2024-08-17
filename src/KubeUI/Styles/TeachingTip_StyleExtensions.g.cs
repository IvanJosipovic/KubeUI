using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TeachingTip = FluentAvalonia.UI.Controls.TeachingTip;

namespace Avalonia.Markup.Declarative;
public static partial class TeachingTipExtensions
{
public static Style<T> Title<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.TitleProperty, value);
public static Style<T> Title<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.TitleProperty, binding);
public static Style<T> Subtitle<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.SubtitleProperty, value);
public static Style<T> Subtitle<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.SubtitleProperty, binding);
public static Style<T> IsOpen<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.IsOpenProperty, value);
public static Style<T> IsOpen<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.IsOpenProperty, binding);
public static Style<T> Target<T>(this Style<T> style, Avalonia.Controls.Control value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.TargetProperty, value);
public static Style<T> Target<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.TargetProperty, binding);
public static Style<T> TailVisibility<T>(this Style<T> style, FluentAvalonia.UI.Controls.TeachingTipTailVisibility value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.TailVisibilityProperty, value);
public static Style<T> TailVisibility<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.TailVisibilityProperty, binding);
public static Style<T> ActionButtonContent<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonContentProperty, value);
public static Style<T> ActionButtonContent<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonContentProperty, binding);
public static Style<T> ActionButtonStyle<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonStyleProperty, value);
public static Style<T> ActionButtonStyle<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonStyleProperty, binding);
public static Style<T> ActionButtonCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandProperty, value);
public static Style<T> ActionButtonCommand<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandProperty, binding);
public static Style<T> ActionButtonCommandParameter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandParameterProperty, value);
public static Style<T> ActionButtonCommandParameter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.ActionButtonCommandParameterProperty, binding);
public static Style<T> CloseButtonContent<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonContentProperty, value);
public static Style<T> CloseButtonContent<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonContentProperty, binding);
public static Style<T> CloseButtonStyle<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonStyleProperty, value);
public static Style<T> CloseButtonStyle<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonStyleProperty, binding);
public static Style<T> CloseButtonCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandProperty, value);
public static Style<T> CloseButtonCommand<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandProperty, binding);
public static Style<T> CloseButtonCommandParameter<T>(this Style<T> style, System.Object value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandParameterProperty, value);
public static Style<T> CloseButtonCommandParameter<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.CloseButtonCommandParameterProperty, binding);
public static Style<T> PlacementMargin<T>(this Style<T> style, Avalonia.Thickness value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.PlacementMarginProperty, value);
public static Style<T> PlacementMargin<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.PlacementMarginProperty, binding);

public static Style<T> PlacementMargin<T>(this Style<T> style, Double uniformLength) where T : FluentAvalonia.UI.Controls.TeachingTip
   => style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.PlacementMarginProperty, new Avalonia.Thickness(uniformLength));
public static Style<T> PlacementMargin<T>(this Style<T> style, Double horizontal, Double vertical) where T : FluentAvalonia.UI.Controls.TeachingTip
   => style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.PlacementMarginProperty, new Avalonia.Thickness(horizontal, vertical));
public static Style<T> PlacementMargin<T>(this Style<T> style, Double left, Double top, Double right, Double bottom) where T : FluentAvalonia.UI.Controls.TeachingTip
   => style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.PlacementMarginProperty, new Avalonia.Thickness(left, top, right, bottom));
public static Style<T> ShouldConstrainToRootBounds<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.ShouldConstrainToRootBoundsProperty, value);
public static Style<T> ShouldConstrainToRootBounds<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.ShouldConstrainToRootBoundsProperty, binding);
public static Style<T> IsLightDismissEnabled<T>(this Style<T> style, System.Boolean value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.IsLightDismissEnabledProperty, value);
public static Style<T> IsLightDismissEnabled<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.IsLightDismissEnabledProperty, binding);
public static Style<T> PreferredPlacement<T>(this Style<T> style, FluentAvalonia.UI.Controls.TeachingTipPlacementMode value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.PreferredPlacementProperty, value);
public static Style<T> PreferredPlacement<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.PreferredPlacementProperty, binding);
public static Style<T> HeroContentPlacement<T>(this Style<T> style, FluentAvalonia.UI.Controls.TeachingTipHeroContentPlacementMode value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.HeroContentPlacementProperty, value);
public static Style<T> HeroContentPlacement<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.HeroContentPlacementProperty, binding);
public static Style<T> HeroContent<T>(this Style<T> style, Avalonia.Controls.Control value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.HeroContentProperty, value);
public static Style<T> HeroContent<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.HeroContentProperty, binding);
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.IconSourceProperty, value);
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.TeachingTip
=> style._addSetter(FluentAvalonia.UI.Controls.TeachingTip.IconSourceProperty, binding);
}

