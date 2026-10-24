namespace KubeUI.Avalonia.Infrastructure.Presentation;

/// <summary>Declares a view and view-model pair for generated view-factory discovery.</summary>
public interface IViewFactoryDefinition
{
    /// <summary>Determines whether the implementing view accepts the supplied view model.</summary>
    /// <param name="viewModel">Candidate view model.</param>
    /// <returns><see langword="true"/> when the view accepts the supplied view model.</returns>
    static abstract bool CanCreate(object viewModel);
}

/// <summary>Provides the view model type for generated view-factory matching.</summary>
/// <typeparam name="TViewModel">View model accepted by the view.</typeparam>
public interface IViewFactoryDefinition<TViewModel> : IViewFactoryDefinition
    where TViewModel : class
{
    static bool IViewFactoryDefinition.CanCreate(object viewModel) => viewModel is TViewModel;
}

internal interface IViewFactory
{
    bool CanCreate(object viewModel);

    Control Create(IServiceProvider serviceProvider);
}

internal sealed class DiscoveredViewFactory(
    Func<object, bool> canCreate,
    Func<IServiceProvider, Control> create) : IViewFactory
{
    public bool CanCreate(object viewModel) => canCreate(viewModel);

    public Control Create(IServiceProvider serviceProvider) => create(serviceProvider);
}
