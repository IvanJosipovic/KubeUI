using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Declarative;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Configuration.v1.Secret;

public sealed partial class CertificateItemView : UserControl, IDeclarativeViewBase
{
    [GeneratedDirectProperty]
    public partial string Header { get; set; }

    [GeneratedDirectProperty]
    public partial byte[] Bytes { get; set; }

    [GeneratedDirectProperty]
    public partial bool HasCert { get; set; }

    public ObservableCollection<X509Certificate2> Certificates { get; } = [];
    public ObservableCollection<RSA> Rsa { get; } = [];
    public ObservableCollection<ECDsa> Ecdsa { get; } = [];

    private readonly ExpandableSection _section;

    public CertificateItemView()
    {
        _section = new ExpandableSection()
            .Header(this, x => x.Header)
            .IsExpanded(true)
            .IsVisible(HasCert)
            .Content(
                new StackPanel()
                    .Children(
                        CreateItems(Certificates, CreateCertificateTemplate),
                        CreateItems(Rsa, CreateKeyTemplate),
                        CreateItems(Ecdsa, CreateKeyTemplate)));

        this.Content(_section);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        ConvertBytes();
    }

    private static ItemsControl CreateItems<T>(IEnumerable<T> items, Func<T, Control> template)
    {
        return new ItemsControl()
            .ItemsSource(items)
            .ItemTemplate(new FuncDataTemplate<T>((item, _) => template(item)));
    }

    private static StackPanel CreateCertificateTemplate(X509Certificate2 cert)
    {
        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.CertificateItemView_Issuer)
                    .Value(cert.Issuer),
                new PropertyItem()
                    .Key(Assets.Resources.CertificateItemView_Subject)
                    .Value(cert.SubjectName.Name),
                new PropertyItem()
                    .Key(Assets.Resources.CertificateItemView_NotBefore)
                    .Value(cert.NotBefore),
                new PropertyItem()
                    .Key(Assets.Resources.CertificateItemView_Expires)
                    .Value(GetExpiryValue(cert)),
                CreateSanList(GetDnsNames(cert), Assets.Resources.CertificateItemView_Dns),
                CreateSanList(GetIpAddresses(cert), Assets.Resources.CertificateItemView_Ip));
    }

    private static StackPanel CreateKeyTemplate(RSA key)
    {
        return CreateKeyTemplate(key.KeyExchangeAlgorithm, key.SignatureAlgorithm, key.KeySize);
    }

    private static StackPanel CreateKeyTemplate(ECDsa key)
    {
        return CreateKeyTemplate(key.KeyExchangeAlgorithm, key.SignatureAlgorithm, key.KeySize);
    }

    private static ItemsControl CreateSanList(IEnumerable<string> values, string key)
    {
        return new ItemsControl()
            .ItemsSource(values)
            .ItemTemplate(new FuncDataTemplate<string>((value, _) => new PropertyItem().Key(key).Value(value)));
    }

    private static string GetExpiryValue(X509Certificate2 cert)
    {
        var expires = cert.NotAfter.ToLocalTime();
        int days = (expires - DateTime.Now).Days;
        return $"{expires} (Valid:{days} days)";
    }

    private static string[] GetDnsNames(X509Certificate2 cert)
    {
        var san = cert.Extensions
            .OfType<X509SubjectAlternativeNameExtension>()
            .FirstOrDefault();

        return san?.EnumerateDnsNames().ToArray() ?? [];
    }

    private static string[] GetIpAddresses(X509Certificate2 cert)
    {
        var san = cert.Extensions
            .OfType<X509SubjectAlternativeNameExtension>()
            .FirstOrDefault();

        return san?.EnumerateIPAddresses().Select(ip => ip.ToString()).ToArray() ?? [];
    }

    private void ConvertBytes()
    {
        if (Bytes is null || Bytes.Length == 0)
        {
            ClearAll();
            return;
        }

        ClearAll();

        string text = Encoding.UTF8.GetString(Bytes);
        Span<byte> buffer = stackalloc byte[Encoding.UTF8.GetByteCount(text)];
        byte[] bytes = Bytes;

        if (Convert.TryFromBase64String(text, buffer, out int decodedBytes))
        {
            text = Encoding.UTF8.GetString(buffer[..decodedBytes]);
            bytes = Encoding.UTF8.GetBytes(text);
        }

        TryAdd(() => Certificates.Add(X509CertificateLoader.LoadCertificate(bytes)));
        TryAdd(() =>
        {
            RSA key = RSA.Create();
            key.ImportRSAPrivateKey(bytes, out _);
            Rsa.Add(key);
        });
        TryAdd(() =>
        {
            RSA key = RSA.Create();
            key.ImportFromPem(Encoding.UTF8.GetString(bytes));
            Rsa.Add(key);
        });
        TryAdd(() =>
        {
            RSA key = RSA.Create();
            key.ImportPkcs8PrivateKey(bytes, out _);
            Rsa.Add(key);
        });
        TryAdd(() =>
        {
            RSA key = RSA.Create();
            key.ImportRSAPublicKey(bytes, out _);
            Rsa.Add(key);
        });
        TryAdd(() =>
        {
            RSA key = RSA.Create();
            key.ImportSubjectPublicKeyInfo(bytes, out _);
            Rsa.Add(key);
        });
        TryAdd(() =>
        {
            ECDsa key = ECDsa.Create();
            key.ImportECPrivateKey(bytes, out _);
            Ecdsa.Add(key);
        });
        TryAdd(() =>
        {
            ECDsa key = ECDsa.Create();
            key.ImportFromPem(Encoding.UTF8.GetString(bytes));
            Ecdsa.Add(key);
        });

        HasCert = Certificates.Any() || Rsa.Any() || Ecdsa.Any();
    }

    private static StackPanel CreateKeyTemplate(string exchangeAlgorithm, string signatureAlgorithm, int keySize)
    {
        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.CertificateItemView_KeyExchangeAlgorithm)
                    .Value(exchangeAlgorithm),
                new PropertyItem()
                    .Key(Assets.Resources.CertificateItemView_SignatureAlgorithm)
                    .Value(signatureAlgorithm),
                new PropertyItem()
                    .Key(Assets.Resources.CertificateItemView_KeySize)
                    .Value(keySize));
    }

    private static void TryAdd(Action action)
    {
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception
        try
        {
            action();
        }
        catch (Exception)
        {
        }
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception
    }

    private void ClearAll()
    {
        Certificates.Clear();
        Rsa.Clear();
        Ecdsa.Clear();
        HasCert = false;
    }
}
