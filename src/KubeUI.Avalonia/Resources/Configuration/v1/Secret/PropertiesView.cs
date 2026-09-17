using System.Text;
using Avalonia.Controls.Templates;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Properties;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Configuration.v1.Secret;

public sealed class PropertiesView : ViewBase<V1Secret>, IResourcePropertiesRefreshable<V1Secret>
{
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
    private DataDisplay<V1Secret, byte[]>? _dataDisplay;
    private ItemsControl? _certificateItems;

    protected override object Build(V1Secret vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        _dataDisplay = new DataDisplay<V1Secret, byte[]>(
            vm,
            GroupApiVersionKind.From<V1Secret>(),
            static resource => resource.Data,
            FormatValue,
            EncodeValue,
            static value => Convert.ToBase64String(value));
        _certificateItems = CreateCertificateItems(vm.Data);

        return new StackPanel()
            .Children(
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Data)
                    .IsExpanded(true)
                    .Content(_dataDisplay),
                new ExpandableSection()
                    .Header(Assets.Resources.SecretPropertiesView_Certificates)
                    .IsExpanded(true)
                    .Content(_certificateItems));
    }

    public void Refresh(V1Secret resource)
    {
        _dataDisplay?.Refresh(resource);
        if (_certificateItems is not null)
        {
            _certificateItems.ItemsSource = resource.Data ?? new Dictionary<string, byte[]>();
        }
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

    private static string FormatValue(byte[] value)
    {
        try
        {
            var text = StrictUtf8.GetString(value);
            return text;
        }
        catch (DecoderFallbackException)
        {
            return Convert.ToBase64String(value);
        }
    }

    private static string EncodeValue(string value)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    }
}
