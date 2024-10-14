#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
[global::System.CodeDom.Compiler.GeneratedCode("AvaloniaExtensionGenerator", "1.0.0.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class PickerFlyoutPresenter_MarkupExtensions
{
//================= Events ======================//
 // Confirmed

/*ActionToEventGenerator*/
public static T OnConfirmed<T>(this T control, Action<FluentAvalonia.UI.Controls.PickerFlyoutPresenter, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.PickerFlyoutPresenter  => 
 control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.PickerFlyoutPresenter,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Confirmed += h);


 // Dismissed

/*ActionToEventGenerator*/
public static T OnDismissed<T>(this T control, Action<FluentAvalonia.UI.Controls.PickerFlyoutPresenter, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.PickerFlyoutPresenter  => 
 control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.PickerFlyoutPresenter,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Dismissed += h);



}
