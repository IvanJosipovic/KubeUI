using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using LoadingContainer = Ursa.Controls.LoadingContainer;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ursa.Controls;

namespace Avalonia.Markup.Declarative;
public static partial class LoadingContainerExtensions
{
public static Style<T> Indicator<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.LoadingContainer
=> style._addSetter(Ursa.Controls.LoadingContainer.IndicatorProperty, value);
public static Style<T> Indicator<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.LoadingContainer
=> style._addSetter(Ursa.Controls.LoadingContainer.IndicatorProperty, binding);
public static Style<T> LoadingMessage<T>(this Style<T> style, System.Object value) where T : Ursa.Controls.LoadingContainer
=> style._addSetter(Ursa.Controls.LoadingContainer.LoadingMessageProperty, value);
public static Style<T> LoadingMessage<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.LoadingContainer
=> style._addSetter(Ursa.Controls.LoadingContainer.LoadingMessageProperty, binding);
public static Style<T> LoadingMessageTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Ursa.Controls.LoadingContainer
=> style._addSetter(Ursa.Controls.LoadingContainer.LoadingMessageTemplateProperty, value);
public static Style<T> LoadingMessageTemplate<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.LoadingContainer
=> style._addSetter(Ursa.Controls.LoadingContainer.LoadingMessageTemplateProperty, binding);
public static Style<T> IsLoading<T>(this Style<T> style, System.Boolean value) where T : Ursa.Controls.LoadingContainer
=> style._addSetter(Ursa.Controls.LoadingContainer.IsLoadingProperty, value);
public static Style<T> IsLoading<T>(this Style<T> style, IBinding binding) where T : Ursa.Controls.LoadingContainer
=> style._addSetter(Ursa.Controls.LoadingContainer.IsLoadingProperty, binding);
}

