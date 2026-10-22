namespace KubeUI.Avalonia.Features.Helm.Repositories;

public sealed class HelmRepositoryEditorView : ViewBase<HelmRepositoryEditorViewModel>
{
    protected override object Build(HelmRepositoryEditorViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new Grid()
            .Rows("Auto,Auto,Auto,Auto,Auto,Auto,Auto")
            .Cols("Auto,*")
            .Margin(16)
            .RowSpacing(8)
            .ColumnSpacing(12)
            .Children(
                new TextBlock().Row(0).Col(0).Text(Assets.Resources.HelmRepositoryEditorView_Name),
                new TextBox().Row(0).Col(1).Text(vm, x => x.Name, BindingMode.TwoWay),
                new TextBlock().Row(1).Col(0).Text(Assets.Resources.HelmRepositoryEditorView_Url),
                new TextBox().Row(1).Col(1).Text(vm, x => x.Url, BindingMode.TwoWay),
                new TextBlock().Row(2).Col(0).Text(Assets.Resources.HelmRepositoryEditorView_Username),
                new TextBox().Row(2).Col(1).Text(vm, x => x.Username, BindingMode.TwoWay),
                new TextBlock().Row(3).Col(0).Text(Assets.Resources.HelmRepositoryEditorView_Password),
                new TextBox().Row(3).Col(1).PasswordChar('●').Text(vm, x => x.Password, BindingMode.TwoWay),
                new TextBlock()
                    .Row(4)
                    .ColSpan(2)
                    .Text(Assets.Resources.HelmRepositoryEditorView_CredentialsNote),
                new TextBlock()
                    .Row(5)
                    .ColSpan(2)
                    .Foreground(Brushes.Firebrick)
                    .Text(vm, x => x.Error),
                new StackPanel()
                    .Row(6)
                    .ColSpan(2)
                    .Orientation(Orientation.Horizontal)
                    .HorizontalAlignment(HorizontalAlignment.Right)
                    .Spacing(8)
                    .Children(
                        new Button()
                            .Command(vm, x => x.CancelCommand)
                            .Content(Assets.Resources.Shared_Cancel),
                        new Button()
                            .Command(vm, x => x.SaveCommand)
                            .Content(Assets.Resources.HelmRepositoryEditorView_Save)));
    }
}
