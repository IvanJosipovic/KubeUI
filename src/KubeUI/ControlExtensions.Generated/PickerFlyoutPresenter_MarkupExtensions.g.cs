#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class PickerFlyoutPresenter_MarkupExtensions
{
//================= Properties ======================//

//================= Events ======================//
 // Confirmed

/*ActionToEventGenerator*/
    public static T OnConfirmed<T>(this T control, Action<FluentAvalonia.UI.Controls.PickerFlyoutPresenter, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.PickerFlyoutPresenter => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.PickerFlyoutPresenter,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Confirmed += h);


 // Dismissed

/*ActionToEventGenerator*/
    public static T OnDismissed<T>(this T control, Action<FluentAvalonia.UI.Controls.PickerFlyoutPresenter, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.PickerFlyoutPresenter => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.PickerFlyoutPresenter,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Dismissed += h);



//================= Styles ======================//

}
