using System.Windows.Input;

namespace KubeUI.DynamicTableView.Tests.Fixtures;

internal sealed class CallbackCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public int ExecutionCount { get; private set; }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => ExecutionCount++;

    public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
