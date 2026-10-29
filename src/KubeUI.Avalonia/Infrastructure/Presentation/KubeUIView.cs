namespace KubeUI.Avalonia.Infrastructure.Presentation;

/// <summary>Base class for views automatically mapped to their view model through generated discovery.</summary>
/// <typeparam name="TViewModel">View model rendered by the view.</typeparam>
public abstract class KubeUIView<TViewModel> : ViewBase<TViewModel>, IViewFactoryDefinition<TViewModel>
    where TViewModel : class
{
}
