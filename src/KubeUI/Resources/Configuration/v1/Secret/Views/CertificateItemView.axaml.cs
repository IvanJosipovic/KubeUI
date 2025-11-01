using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace KubeUI.Resources.Configuration.v1.Secret.Views;

public partial class CertificateItemView : UserControl
{
    [GeneratedDirectProperty]
    public partial string Header { get; set; }

    [GeneratedDirectProperty]
    public partial byte[] Bytes { get; set; }

    [GeneratedDirectProperty]
    public partial bool HasCert { get; set; }

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
        if (Bytes is null || Bytes.Length == 0)
        {
            ClearAll();
            return;
        }

        ClearAll();

        var text = Encoding.UTF8.GetString(Bytes);
        Span<byte> buffer = stackalloc byte[Encoding.UTF8.GetByteCount(text)];
        var bytes = Bytes;

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
