using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls.Primitives;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TaskDialogCommandHost = FluentAvalonia.UI.Controls.Primitives.TaskDialogCommandHost;

namespace Avalonia.Markup.Declarative;
public static partial class TaskDialogCommandHostExtensions
{
public static Style<T> Description<T>(this Style<T> style, System.String value) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogCommandHost
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.TaskDialogCommandHost.DescriptionProperty, value);
public static Style<T> Description<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogCommandHost
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.TaskDialogCommandHost.DescriptionProperty, binding);
}

