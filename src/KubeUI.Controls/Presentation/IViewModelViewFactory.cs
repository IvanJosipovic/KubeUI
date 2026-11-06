namespace KubeUI.Controls.Presentation;

/// <summary>Creates a view for a view model using the application's service provider.</summary>
public interface IViewModelViewFactory
{
    /// <summary>Creates the view associated with this view model.</summary>
    /// <param name="serviceProvider">Application services used to construct the view.</param>
    /// <returns>Created view.</returns>
    Control CreateView(IServiceProvider serviceProvider);
}
