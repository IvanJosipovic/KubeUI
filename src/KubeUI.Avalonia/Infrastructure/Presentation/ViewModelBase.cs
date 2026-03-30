using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Presentation;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;

namespace KubeUI.Avalonia.Infrastructure.Presentation;

public abstract class ViewModelBase : Tool
{
    public new IFactory Factory { get; set; } = Application.Current.GetRequiredService<IFactory>();

    public bool IsPinned => Factory.IsDockablePinned(this);
}

