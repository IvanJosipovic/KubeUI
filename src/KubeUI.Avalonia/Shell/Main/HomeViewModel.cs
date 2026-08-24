using KubeUI.Avalonia.Infrastructure.Presentation;

namespace KubeUI.Avalonia.Shell.Main;

public sealed partial class HomeViewModel : ViewModelBase
{
    public HomeViewModel()
    {
        Title = Assets.Resources.HomeView_Title;
        Id = nameof(HomeViewModel);
    }

}

