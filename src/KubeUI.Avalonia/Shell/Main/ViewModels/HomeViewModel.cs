namespace KubeUI.Avalonia.Shell.Main.ViewModels;

public sealed partial class HomeViewModel : ViewModelBase
{
    public HomeViewModel()
    {
        Title = Assets.Resources.HomeViewModel_Title;
        Id = nameof(HomeViewModel);
    }
}

