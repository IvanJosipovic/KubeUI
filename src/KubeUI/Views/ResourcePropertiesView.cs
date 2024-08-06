using k8s.Models;
using k8s;
using Avalonia.Controls.Templates;
using System.Text;

namespace KubeUI.Views;

public sealed class ResourcePropertiesView<T> : MyViewBase<ResourcePropertiesViewModel<T>> where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    protected override StyleGroup? BuildStyles() => [];

    protected override object Build(ResourcePropertiesViewModel<T>? vm)
    {
        if (vm == null) return new StackPanel();

        var sp = new StackPanel()
            .Children([
                new Grid().Cols("*,3*").VerticalAlignment(VerticalAlignment.Top)
                    .Children([
                        new SelectableTextBlock().Row(0).Col(0)
                            .Text("Name:"),
                        new SelectableTextBlock().Row(0).Col(1)
                            .Text(@vm.Object.Metadata.Name),
                        ]),
                new Grid().Cols("*,3*").VerticalAlignment(VerticalAlignment.Top)
                    .Children([
                        new SelectableTextBlock().Row(1).Col(0)
                            .Text("Namespace:"),
                        new SelectableTextBlock().Row(1).Col(1)
                            .Text(@vm.Object.Metadata.NamespaceProperty),
                        ]),
                ]);


        if(typeof(T) == typeof(V1Secret))
        {
            var obj = vm.Object as V1Secret;

            sp.Children.AddRange([
                new Separator(),
                new ItemsControl()
                    .ItemsSource(@obj.Data)
                    .ItemTemplate(new FuncDataTemplate<KeyValuePair<string, byte[]>>((x,_) =>
                        new Grid().Cols("*,2*").VerticalAlignment(VerticalAlignment.Top)
                            .Children([
                                new SelectableTextBlock().Row(0).Col(0)
                                    .Text(x.Key),
                                new SelectableTextBlock().Row(0).Col(1)
                                    .Text(Encoding.UTF8.GetString(x.Value), ps: "Value"),
                            ])
                        )
                    )
            ]);
        }

        return sp;
    }
}
