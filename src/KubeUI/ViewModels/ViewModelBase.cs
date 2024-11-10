using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;

namespace KubeUI.ViewModels;

public abstract class ViewModelBase : Tool
{
    public new IFactory Factory { get; set; } = Application.Current.GetRequiredService<IFactory>();

    public bool IsPinned => Factory.IsDockablePinned(this);
}
