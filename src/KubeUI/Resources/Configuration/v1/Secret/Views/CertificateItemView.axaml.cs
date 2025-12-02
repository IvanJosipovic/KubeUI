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

    public ObservableCollection<X509Certificate2> Certificates { get; } = [];
    public ObservableCollection<RSA> Rsa { get; } = [];
    public ObservableCollection<ECDsa> Ecdsa { get; } = [];

    public CertificateItemView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        ConvertBytes();
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
            var cert = X509CertificateLoader.LoadCertificate(bytes);
            Certificates.Add(cert);
        }
        catch (Exception) {}

        try
        {
            var key = RSA.Create();
            key.ImportRSAPrivateKey(bytes, out _);
            Rsa.Add(key);
        }
        catch (Exception) {}

        try
        {
            var key = RSA.Create();
            var str = Encoding.UTF8.GetString(bytes);
            key.ImportFromPem(str);
            Rsa.Add(key);
        }
        catch (Exception) { }

        try
        {
            var key = RSA.Create();
            key.ImportPkcs8PrivateKey(bytes, out _);
            Rsa.Add(key);
        }
        catch (Exception) { }

        try
        {
            var key = RSA.Create();
            key.ImportRSAPublicKey(bytes, out _);
            Rsa.Add(key);
        }
        catch (Exception) { }

        try
        {
            var key = RSA.Create();
            key.ImportSubjectPublicKeyInfo(bytes, out _);
            Rsa.Add(key);
        }
        catch (Exception) { }

        try
        {
            var key = ECDsa.Create();
            key.ImportECPrivateKey(bytes, out _);
            Ecdsa.Add(key);
        }
        catch (Exception) { }

        try
        {
            var key = ECDsa.Create();
            var str = Encoding.UTF8.GetString(bytes);
            key.ImportFromPem(str);
            Ecdsa.Add(key);
        }
        catch (Exception) { }

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
