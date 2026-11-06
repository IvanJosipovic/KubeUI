using KubeUI.App.Infrastructure.Presentation;

namespace KubeUI.App.Shell.Main;

public sealed partial class HomeViewModel : ViewModelBase
{
    public HomeViewModel()
    {
        Title = Assets.Resources.HomeView_Title;
        Id = nameof(HomeViewModel);
    }

}

