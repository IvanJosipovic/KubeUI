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
public static partial class Layout_MarkupExtensions
{
//================= Properties ======================//

//================= Events ======================//
 // MeasureInvalidated

/*ActionToEventGenerator*/
    public static T OnMeasureInvalidated<T>(this T control, Action<FluentAvalonia.UI.Controls.Layout, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.Layout => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.Layout,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.MeasureInvalidated += h);


 // ArrangeInvalidated

/*ActionToEventGenerator*/
    public static T OnArrangeInvalidated<T>(this T control, Action<FluentAvalonia.UI.Controls.Layout, System.EventArgs> action) where T : FluentAvalonia.UI.Controls.Layout => 
        control._setEvent((FluentAvalonia.Core.TypedEventHandler<FluentAvalonia.UI.Controls.Layout,System.EventArgs>) ((arg0, arg1) => action(arg0, arg1)), h => control.ArrangeInvalidated += h);



//================= Styles ======================//

}
