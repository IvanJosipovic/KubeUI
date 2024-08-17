using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using DataGridDetailsPresenter = Avalonia.Controls.Primitives.DataGridDetailsPresenter;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
public static partial class DataGridDetailsPresenterExtensions
{
public static Style<Avalonia.Controls.Primitives.DataGridDetailsPresenter> ContentHeight(this Style<Avalonia.Controls.Primitives.DataGridDetailsPresenter> style, System.Double value)
=> style._addSetter(Avalonia.Controls.Primitives.DataGridDetailsPresenter.ContentHeightProperty, value);
public static Style<Avalonia.Controls.Primitives.DataGridDetailsPresenter> ContentHeight(this Style<Avalonia.Controls.Primitives.DataGridDetailsPresenter> style, IBinding binding)
=> style._addSetter(Avalonia.Controls.Primitives.DataGridDetailsPresenter.ContentHeightProperty, binding);
}

