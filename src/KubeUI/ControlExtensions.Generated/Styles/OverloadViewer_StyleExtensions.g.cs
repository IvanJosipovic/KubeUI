using Avalonia.Data;
using Avalonia.Data.Converters;
using AvaloniaEdit.CodeCompletion;
using OverloadViewer = AvaloniaEdit.CodeCompletion.OverloadViewer;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class OverloadViewerExtensions
{
public static Style<T> Text<T>(this Style<T> style, System.String value) where T : AvaloniaEdit.CodeCompletion.OverloadViewer
=> style._addSetter(AvaloniaEdit.CodeCompletion.OverloadViewer.TextProperty, value);
public static Style<T> Text<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.CodeCompletion.OverloadViewer
=> style._addSetter(AvaloniaEdit.CodeCompletion.OverloadViewer.TextProperty, binding);
public static Style<T> Provider<T>(this Style<T> style, AvaloniaEdit.CodeCompletion.IOverloadProvider value) where T : AvaloniaEdit.CodeCompletion.OverloadViewer
=> style._addSetter(AvaloniaEdit.CodeCompletion.OverloadViewer.ProviderProperty, value);
public static Style<T> Provider<T>(this Style<T> style, IBinding binding) where T : AvaloniaEdit.CodeCompletion.OverloadViewer
=> style._addSetter(AvaloniaEdit.CodeCompletion.OverloadViewer.ProviderProperty, binding);
}

