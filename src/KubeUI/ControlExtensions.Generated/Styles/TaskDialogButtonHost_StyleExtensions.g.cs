using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Controls.Primitives;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using TaskDialogButtonHost = FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonHost;

namespace Avalonia.Markup.Declarative;
public static partial class TaskDialogButtonHostExtensions
{
public static Style<T> IconSource<T>(this Style<T> style, FluentAvalonia.UI.Controls.IconSource value) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonHost
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonHost.IconSourceProperty, value);
public static Style<T> IconSource<T>(this Style<T> style, IBinding binding) where T : FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonHost
=> style._addSetter(FluentAvalonia.UI.Controls.Primitives.TaskDialogButtonHost.IconSourceProperty, binding);
}

