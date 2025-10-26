// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace Avalonia.Xaml.Interactions.Custom;

/// <summary>
/// Sets the <see cref="ToolTip.TipProperty"/> of the associated or target control when executed.
/// </summary>
public class SetToolTipTipAction : StyledElementAction, IAction
{
    /// <summary>
    /// Identifies the <seealso cref="TargetControl"/> avalonia property.
    /// </summary>
    public static readonly StyledProperty<Control?> TargetControlProperty =
        AvaloniaProperty.Register<SetToolTipTipAction, Control?>(nameof(TargetControl));

    /// <summary>
    /// Identifies the <seealso cref="Tip"/> avalonia property.
    /// </summary>
    public static readonly StyledProperty<object?> TipProperty =
        AvaloniaProperty.Register<SetToolTipTipAction, object?>(nameof(Tip));

    /// <summary>
    /// Gets or sets the control whose tooltip will be updated. This is an avalonia property.
    /// </summary>
    [ResolveByName]
    public Control? TargetControl
    {
        get => GetValue(TargetControlProperty);
        set => SetValue(TargetControlProperty, value);
    }

    /// <summary>
    /// Gets or sets the new tooltip content. This is an avalonia property.
    /// </summary>
    public object? Tip
    {
        get => GetValue(TipProperty);
        set => SetValue(TipProperty, value);
    }

    /// <inheritdoc />
    public override object Execute(object? sender, object? parameter)
    {
        if (!IsEnabled)
        {
            return false;
        }

        var control = TargetControl ?? sender as Control;
        if (control is null)
        {
            return false;
        }

        ToolTip.SetTip(control, Tip);
        return true;
    }
}
