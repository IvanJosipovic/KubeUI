using System.Collections.Generic;
using System.Text;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Configuration.v1.Secret;

public sealed class PropertiesView : ViewBase<V1Secret>
{
    protected override object Build(V1Secret vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Data)
                    .IsExpanded(true)
                    .Content(CreateDataItems(vm.Data)),
                new ExpandableSection()
                    .Header(Assets.Resources.SecretPropertiesView_Certificates)
                    .IsExpanded(true)
                    .Content(CreateCertificateItems(vm.Data)));
    }

    private static ItemsControl CreateDataItems(IDictionary<string, byte[]>? data)
    {
        return new ItemsControl()
            .ItemsSource(data ?? new Dictionary<string, byte[]>())
            .ItemTemplate(new FuncDataTemplate<KeyValuePair<string, byte[]>>((entry, _) =>
                new ExpandableSection()
                    .Header(entry.Key)
                    .IsExpanded(true)
                    .Content(
                        new SelectableTextBlock()
                            .Text(Encoding.UTF8.GetString(entry.Value)))));
    }

    private static ItemsControl CreateCertificateItems(IDictionary<string, byte[]>? data)
    {
        return new ItemsControl()
            .ItemsSource(data ?? new Dictionary<string, byte[]>())
            .ItemTemplate(new FuncDataTemplate<KeyValuePair<string, byte[]>>((entry, _) =>
                new CertificateItemView()
                    .Header(entry.Key)
                    .Bytes(entry.Value)));
    }
}
