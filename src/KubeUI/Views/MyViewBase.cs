using Avalonia.Markup.Declarative;

namespace KubeUI.Views;

public abstract class MyViewBase<TViewModel> : ViewBase
{
    public TViewModel? ViewModel
    {
        get => (TViewModel?)DataContext;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext != null)
        {
            OnCreatedCore();
            Initialize();
        }
    }

    protected MyViewBase() : base(deferredLoading: true)
    {
    }

    protected abstract object Build(TViewModel? vm);

    protected override object Build() => Build((TViewModel)DataContext);
}
