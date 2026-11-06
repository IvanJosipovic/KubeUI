namespace KubeUI.Controls.Presentation;

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

/// <summary>Creates a view for matching view models through the owning service provider.</summary>
public interface IViewFactory
{
    /// <summary>Determines whether this factory can create a view for the supplied view model.</summary>
    /// <param name="viewModel">Candidate view model.</param>
    /// <returns><see langword="true"/> when this factory can create a view.</returns>
    bool CanCreate(object viewModel);

    /// <summary>Creates the view using feature-owned service registrations.</summary>
    /// <param name="serviceProvider">Provider that owns the view registration.</param>
    /// <returns>The resolved view.</returns>
    Control Create(IServiceProvider serviceProvider);
}

/// <summary>Adapts a view's static factory definition and typed service resolution.</summary>
public sealed class DiscoveredViewFactory : IViewFactory
{
    private readonly Func<object, bool> _canCreate;
    private readonly Func<IServiceProvider, Control> _create;

    /// <summary>Creates a view factory from generated matching and resolution delegates.</summary>
    /// <param name="canCreate">Generated predicate that matches a view model.</param>
    /// <param name="create">Generated delegate that resolves the view from its provider.</param>
    public DiscoveredViewFactory(Func<object, bool> canCreate, Func<IServiceProvider, Control> create)
    {
        _canCreate = canCreate ?? throw new ArgumentNullException(nameof(canCreate));
        _create = create ?? throw new ArgumentNullException(nameof(create));
    }

    /// <inheritdoc />
    public bool CanCreate(object viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        return _canCreate(viewModel);
    }

    /// <inheritdoc />
    public Control Create(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        return _create(serviceProvider);
    }
}
