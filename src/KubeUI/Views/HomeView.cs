using KubeUI.ViewModels;

namespace KubeUI.Views;

public sealed class HomeView : MyViewBase<HomeViewModel>
{
    protected override object Build(HomeViewModel vm) =>
        new TextBlock()
            .Margin(5, 5, 0, 0)
            .HorizontalAlignment(HorizontalAlignment.Center)
            .FontSize(24)
            .Text(Assets.Resources.HomeView_Header);
}
