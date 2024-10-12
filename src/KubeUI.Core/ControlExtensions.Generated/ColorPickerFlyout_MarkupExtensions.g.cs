#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
[global::System.CodeDom.Compiler.GeneratedCode("AvaloniaExtensionGenerator", "11.1.3.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class ColorPickerFlyout_MarkupExtensions
{
//================= Properties ======================//

//================= Events ======================//
 // Confirmed

/*ActionToEventGenerator*/
    public static FluentAvalonia.UI.Controls.ColorPickerFlyout OnConfirmed(this FluentAvalonia.UI.Controls.ColorPickerFlyout control, Action<FluentAvalonia.UI.Controls.ColorPickerFlyout, System.EventArgs> action) => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ColorPickerFlyout,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Confirmed += h);


 // Dismissed

/*ActionToEventGenerator*/
    public static FluentAvalonia.UI.Controls.ColorPickerFlyout OnDismissed(this FluentAvalonia.UI.Controls.ColorPickerFlyout control, Action<FluentAvalonia.UI.Controls.ColorPickerFlyout, System.EventArgs> action) => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.ColorPickerFlyout,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.Dismissed += h);



//================= Styles ======================//

}
