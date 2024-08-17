using Avalonia.Data;
using Avalonia.Data.Converters;
using OverlayFeedbackElement = Ursa.Controls.OverlayShared.OverlayFeedbackElement;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls.OverlayShared;

namespace Avalonia.Markup.Declarative;
public static partial class OverlayFeedbackElementExtensions
{
public static Style<T> IsClosed<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.OverlayShared.OverlayFeedbackElement
=> style._addSetter(Ursa.Controls.OverlayShared.OverlayFeedbackElement.IsClosedProperty, value);
public static Style<T> IsClosed<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.OverlayShared.OverlayFeedbackElement
=> style._addSetter(Ursa.Controls.OverlayShared.OverlayFeedbackElement.IsClosedProperty, binding);
}

