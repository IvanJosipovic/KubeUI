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
public static partial class TreeDataGridColumnHeadersPresenter_MarkupExtensions
{
//================= Events ======================//
 // ChildIndexChanged

/*ActionToEventGenerator*/
public static T OnChildIndexChanged<T>(this T control, Action<Avalonia.LogicalTree.ChildIndexChangedEventArgs> action) where T : Avalonia.Controls.Primitives.TreeDataGridColumnHeadersPresenter  => 
 control._setEvent((System.EventHandler<Avalonia.LogicalTree.ChildIndexChangedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.ChildIndexChanged += h);



}
