using KubeUI.Controls.Presentation;

namespace KubeUI.Features.Resources.Workloads.v1.Deployment;

public sealed class DeploymentRolloutHistoryView : KubeUIView<DeploymentRolloutHistoryViewModel>
{
    protected override object Build(DeploymentRolloutHistoryViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new Grid()
            .Rows("*,Auto,Auto")
            .Margin(12)
            .Children(
                new ListBox()
                    .Row(0)
                    .ItemsSource(vm, x => x.Revisions)
                    .SelectedItem(vm, x => x.SelectedRevision, BindingMode.TwoWay)
                    .ItemTemplate<DeploymentRevision>(revision =>
                        new TextBlock()
                            .Text(revision.Display)
                            .TextWrapping(TextWrapping.Wrap)
                            .Margin(6)),
                new TextBlock()
                    .Row(1)
                    .Text(vm, x => x.StatusMessage)
                    .TextWrapping(TextWrapping.Wrap)
                    .Margin(0, 8),
                new Button()
                    .Row(2)
                    .HorizontalAlignment(HorizontalAlignment.Right)
                    .Content(L10n.Instance.DeploymentRolloutHistoryViewModel_Undo.ToBinding())
                    .Command(vm, x => x.UndoCommand));
    }
}
