using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Avalonia;
using Avalonia.Controls;

namespace KubeUI.Resources.Configuration.v1.Secret.Views;

public partial class CertificateItemView : UserControl
{
    public static readonly DirectProperty<CertificateItemView, string> HeaderProperty =
        AvaloniaProperty.RegisterDirect<CertificateItemView, string>(
            nameof(Header),
            o => o.Header,
            (o, v) => o.Header = v);

    private string _header = string.Empty;
    public string Header
    {
        get => _header;
        set => SetAndRaise(HeaderProperty, ref _header, value);
    }

    public static readonly DirectProperty<CertificateItemView, byte[]> BytesProperty =
        AvaloniaProperty.RegisterDirect<CertificateItemView, byte[]>(
            nameof(Bytes),
            o => o.Bytes,
            (o, v) => o.Bytes = v);

    private byte[] _bytes = [];
    public byte[] Bytes
    {
        get => _bytes;
        set
        {
            SetAndRaise(BytesProperty, ref _bytes, value);
            ConvertBytes();
        }
    }

    public static readonly DirectProperty<CertificateItemView, bool> HasCertProperty =
        AvaloniaProperty.RegisterDirect<CertificateItemView, bool>(
            nameof(HasCert),
            o => o.HasCert,
            (o, v) => o.HasCert = v);

    private bool _hasCert;
    public bool HasCert
    {
        get => _hasCert;
        private set => SetAndRaise(HasCertProperty, ref _hasCert, value);
    }

    // Collections exposed for XAML binding
    public ObservableCollection<X509Certificate2> Certificates { get; } = [];
    public ObservableCollection<RSA> Rsa { get; } = [];
    public ObservableCollection<ECDsa> Ecdsa { get; } = [];

    public CertificateItemView()
    {
        InitializeComponent();
    }

    private void ConvertBytes()
    {
        if (_bytes is null || _bytes.Length == 0)
        {
            ClearAll();
            return;
        }

        ClearAll();

        var text = Encoding.UTF8.GetString(_bytes);
        Span<byte> buffer = stackalloc byte[Encoding.UTF8.GetByteCount(text)];
        var bytes = _bytes;

        if (Convert.TryFromBase64String(text, buffer, out var decodedBytes))
        {
            text = Encoding.UTF8.GetString(buffer[..decodedBytes]);
            bytes = Encoding.UTF8.GetBytes(text);
        }

        try
        {
            if (text.StartsWith("-----BEGIN CERTIFICATE-----", StringComparison.Ordinal))
            {
                var cert = X509CertificateLoader.LoadCertificate(bytes);
                Certificates.Add(cert);
            }
            else if (text.StartsWith("-----BEGIN RSA PRIVATE KEY-----", StringComparison.Ordinal))
            {
                var key = RSA.Create();
                key.ImportRSAPrivateKey(bytes, out _);
                Rsa.Add(key);
            }
            else if (text.StartsWith("-----BEGIN PRIVATE KEY-----", StringComparison.Ordinal))
            {
                var key = RSA.Create();
                key.ImportPkcs8PrivateKey(bytes, out _);
                Rsa.Add(key);
            }
            else if (text.StartsWith("-----BEGIN RSA PUBLIC KEY-----", StringComparison.Ordinal))
            {
                var key = RSA.Create();
                key.ImportRSAPublicKey(bytes, out _);
                Rsa.Add(key);
            }
            else if (text.StartsWith("-----BEGIN PUBLIC KEY-----", StringComparison.Ordinal))
            {
                var key = RSA.Create();
                key.ImportSubjectPublicKeyInfo(bytes, out _);
                Rsa.Add(key);
            }
            else if (text.StartsWith("-----BEGIN EC PRIVATE KEY-----", StringComparison.Ordinal))
            {
                var ecdsa = ECDsa.Create();
                ecdsa.ImportECPrivateKey(bytes, out _);
                Ecdsa.Add(ecdsa);
            }
        }
        catch (CryptographicException)
        {
            // Ignore malformed data
        }

        HasCert = Certificates.Any() || Rsa.Any() || Ecdsa.Any();
    }

    private void ClearAll()
    {
        Certificates.Clear();
        Rsa.Clear();
        Ecdsa.Clear();
        HasCert = false;
    }
}
