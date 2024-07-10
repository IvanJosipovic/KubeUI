namespace KubeUI.ViewModels;

public sealed partial class HomeViewModel : ViewModelBase
{
    public HomeViewModel()
    {
        Title = Resources.HomeViewModel_Title;
        Id = nameof(HomeViewModel);
    }
}
