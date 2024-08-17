using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using DocumentControl = Dock.Avalonia.Controls.DocumentControl;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DocumentControlExtensions
{
public static Style<T> HeaderTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Dock.Avalonia.Controls.DocumentControl
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty, value);
public static Style<T> HeaderTemplate<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.HeaderTemplateProperty, binding);
public static Style<T> IsActive<T>(this Style<T> style, System.Boolean value) where T : Dock.Avalonia.Controls.DocumentControl
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty, value);
public static Style<T> IsActive<T>(this Style<T> style, IBinding binding) where T : Dock.Avalonia.Controls.DocumentControl
=> style._addSetter(Dock.Avalonia.Controls.DocumentControl.IsActiveProperty, binding);
}

