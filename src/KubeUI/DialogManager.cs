using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;

namespace KubeUI;

internal class MyDialogManager : DialogManager
{
    public MyDialogManager(IViewLocator? viewLocator = null, IDialogFactory? dialogFactory = null, ILogger<DialogManager>? logger = null, IDispatcher? dispatcher = null, Control? customNavigationRoot = null) : base(viewLocator, dialogFactory, logger, dispatcher, customNavigationRoot)
    {
    }

    public override IView? FindViewByViewModel(INotifyPropertyChanged viewModel)
    {
        return GetMainWindow();
    }
}
