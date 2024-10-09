using k8s.Models;
using KubeUI.Views;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;

namespace KubeUI.Controls;

public sealed class PodContainerCell : MyViewBase<V1Pod>
{
    protected override StyleGroup? BuildStyles() => [
        new Style<Ellipse>()
            .Setter(Shape.FillProperty, Brushes.Red)
            .Setter(Shape.WidthProperty, 10.0)
            .Setter(Shape.HeightProperty, 10.0)
            .Setter(Shape.StrokeProperty, Brushes.Gray)
            .Setter(Shape.StrokeThicknessProperty, 1.0)
            .Setter(Shape.MarginProperty, new Thickness(left: 0.0, top: 0.0, right: 4.0, bottom: 0.0))
        ];

    private static IBrush GetBrush(V1ContainerStatus status , bool isInit = false)
    {
        if (status.Ready && status.Started == true)
        {
            return Brushes.LimeGreen;
        }
        if (!status.Ready && status.Started == true)
        {
            return Brushes.Orange;
        }
        else if (status.State.Waiting != null)
        {
            return Brushes.Orange;
        }
        else if (status.State.Terminated != null)
        {
            if (status.State.Terminated.Reason == "Completed")
            {
                return Brushes.Gray;
            }

            return Brushes.Orange;
        }

        return Brushes.Red;
    }

    protected override object Build(V1Pod? vm) =>
        new StackPanel()
            .Margin(10, 0, 0, 0)
            .Orientation(Orientation.Horizontal)
            .Children([
                new ItemsControl()
                    .ItemsSource(vm.Status.ContainerStatuses)
                    .ItemsPanel(new StackPanel().Orientation(Orientation.Horizontal))
                    .ItemTemplate(new FuncDataTemplate<V1ContainerStatus>((item, ns) => {
                        return new Ellipse()
                                .Fill(GetBrush(item))
                                .ToolTip(new StackPanel()
                                            .Children([
                                                new TextBlock()
                                                    .Text(string.Format("Name: {0}", item.Name)),
                                                ])
                                );
                    })),
                new ItemsControl()
                    .ItemsSource(vm.Status.InitContainerStatuses)
                    .ItemsPanel(new StackPanel().Orientation(Orientation.Horizontal))
                    .ItemTemplate(new FuncDataTemplate<V1ContainerStatus>((item, ns) => {
                        return new Ellipse()
                                .Fill(GetBrush(item, true))
                                .ToolTip(new StackPanel()
                                            .Children([
                                                new TextBlock()
                                                    .Text(string.Format("Name: {0}", item.Name)),
                                                ])
                                );
                    }))
            ]);
}
