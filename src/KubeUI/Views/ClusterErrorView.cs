namespace KubeUI.Views;

public sealed class ClusterErrorView : MyViewBase<ClusterErrorViewModel>
{
    protected override object Build(ClusterErrorViewModel? vm) =>
        new Grid()
            .Rows("Auto, *")
            .Children([
                new TextBlock()
                    .Row(0)
                    .Padding(5,0,0,0)
                    .Text(Assets.Resources.ClusterErrorView_Header),
                new SelectableTextBlock()
                    .Row(1)
                    .Padding(5,0,0,0)
                    .Text(@vm.Error)
                ]);
}
