using KubeUI.App.Infrastructure.Presentation;

namespace KubeUI.App.Shell.Main;

public sealed partial class HomeViewModel : ViewModelBase
{
    public HomeViewModel()
    {
        Title = L10n.Instance.HomeView_Title.CurrentValue()!;
        Id = nameof(HomeViewModel);
    }

}

