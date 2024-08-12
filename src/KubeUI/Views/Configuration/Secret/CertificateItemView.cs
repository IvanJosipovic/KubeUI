using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Security.Cryptography;
using Avalonia.Controls.Templates;

namespace KubeUI.Views;
public sealed partial class CertificateItemView : ViewBase
{
    public static readonly DirectProperty<CertificateItemView, string> HeaderProperty =
    AvaloniaProperty.RegisterDirect<CertificateItemView, string>(
    nameof(Header),
    o => o.Header,
    (o, v) => o.Header = v);

    private string _header = string.Empty;

    public string Header
    {
        get { return _header; }
        set { SetAndRaise(HeaderProperty, ref _header, value); }
    }

    public static readonly DirectProperty<CertificateItemView, byte[]> BytesProperty =
    AvaloniaProperty.RegisterDirect<CertificateItemView, byte[]>(
    nameof(Bytes),
    o => o.Bytes,
    (o, v) => o.Bytes = v);

    private byte[] _bytes;

    public byte[] Bytes
    {
        get { return _bytes; }
        set { SetAndRaise(BytesProperty, ref _bytes, value); Convert(); }
    }

    public static readonly DirectProperty<CertificateItemView, bool> HasCertProperty =
        AvaloniaProperty.RegisterDirect<CertificateItemView, bool>(
        nameof(HasCert),
        o => o.HasCert,
        (o, v) => o.HasCert = v);

    private bool _hasCert;

    public bool HasCert
    {
        get { return _hasCert; }
        set { SetAndRaise(HasCertProperty, ref _hasCert, value);}
    }

    private ObservableCollection<X509Certificate2> Certificates = [];
    private ObservableCollection<RSA> Rsa = [];
    private ObservableCollection<ECDsa> Ecdsa = [];

    private void Convert()
    {
        if (_bytes == null)
        {
            return;
        }

        Certificates.Clear();
        Rsa.Clear();
        Ecdsa.Clear();
        var text = Encoding.UTF8.GetString(Bytes);
        Span<byte> buffer = stackalloc byte[Encoding.UTF8.GetByteCount(text)];
        byte[] bytes = Bytes;

        if (System.Convert.TryFromBase64String(text, buffer, out var decodedBytes))
        {
            text = Encoding.UTF8.GetString(buffer);
            bytes = Encoding.UTF8.GetBytes(text);
        }

        try
        {
            if (text.StartsWith("-----BEGIN CERTIFICATE-----"))
            {
                var cert = new X509Certificate2(bytes);
                Certificates.Add(cert);
            }
            else if (text.StartsWith("-----BEGIN RSA PRIVATE KEY-----"))
            {
                RSA key = RSA.Create();
                key.ImportRSAPrivateKey(bytes, out _);

                Rsa.Add(key);
            }
            else if (text.StartsWith("-----BEGIN PRIVATE KEY-----"))
            {
                RSA key = RSA.Create();
                key.ImportPkcs8PrivateKey(bytes, out _);

                Rsa.Add(key);
            }
            else if (text.StartsWith("-----BEGIN RSA PUBLIC KEY-----"))
            {
                RSA key = RSA.Create();
                key.ImportRSAPublicKey(bytes, out _);

                Rsa.Add(key);
            }
            else if (text.StartsWith("-----BEGIN PUBLIC KEY-----"))
            {
                RSA key = RSA.Create();
                key.ImportSubjectPublicKeyInfo(bytes, out _);

                Rsa.Add(key);
            }
            else if (text.StartsWith("-----BEGIN EC PRIVATE KEY-----"))
            {
                var ecdsa = ECDsa.Create();
                ecdsa.ImportECPrivateKey(bytes, out _);

                Ecdsa.Add(ecdsa);
            }
        }
        catch (CryptographicException) { }

        HasCert = Certificates.Any() || Rsa.Any() || Ecdsa.Any();
    }

    protected override object Build() =>
        new HeaderItem()
            .IsVisible(@HasCert)
            .DataContext(this)
            .Text(@Header)
            .Controls([
                new StackPanel()
                    .Children([
                        new ItemsControl()
                            .ItemsSource(@Certificates)
                            .ItemTemplate(new FuncDataTemplate<X509Certificate2>((x,_) =>
                                new StackPanel()
                                    .Children([
                                        new PropertyItem().Key("Issuer").Value(x.Issuer),
                                        new PropertyItem().Key("Subject").Value(x.SubjectName.Name),
                                        new PropertyItem().Key("Not Before").Value(x.NotBefore.ToLocalTime().ToString()),
                                        new PropertyItem().Key("Expires").Value($"{x.NotAfter.ToLocalTime()} (Valid:{(x.NotAfter.ToLocalTime() - DateTime.Now).Days} days)"),
                                        new ItemsControl()
                                            .ItemsSource(x.Extensions.OfType<X509SubjectAlternativeNameExtension>().FirstOrDefault()?.EnumerateDnsNames() ?? [])
                                            .ItemTemplate(new FuncDataTemplate<string>((x,_) =>
                                                new StackPanel()
                                                    .Children([
                                                        new PropertyItem().Key("DNS").Value(x),
                                                    ])
                                        )),
                                        new ItemsControl()
                                            .ItemsSource(x.Extensions.OfType<X509SubjectAlternativeNameExtension>().FirstOrDefault()?.EnumerateIPAddresses() ?? [])
                                            .ItemTemplate(new FuncDataTemplate<string>((x,_) =>
                                                new StackPanel()
                                                    .Children([
                                                        new PropertyItem().Key("IP").Value(x),
                                                    ])
                                        )),
                                    ])
                        )),
                        new ItemsControl()
                            .ItemsSource(@Rsa)
                            .ItemTemplate(new FuncDataTemplate<RSA>((x,_) =>
                                new StackPanel()
                                    .Children([
                                        new PropertyItem().Key("Key Exchange Algorithm").Value(x.KeyExchangeAlgorithm),
                                        new PropertyItem().Key("Signature Algorithm").Value(x.SignatureAlgorithm),
                                        new PropertyItem().Key("Key Size").Value(x.KeySize.ToString()),
                                    ])
                        )),
                        new ItemsControl()
                            .ItemsSource(@Ecdsa)
                            .ItemTemplate(new FuncDataTemplate<ECDsa>((x,_) =>
                                new StackPanel()
                                    .Children([
                                        new PropertyItem().Key("Key Exchange Algorithm").Value(x.KeyExchangeAlgorithm),
                                        new PropertyItem().Key("Signature Algorithm").Value(x.SignatureAlgorithm),
                                        new PropertyItem().Key("Key Size").Value(x.KeySize.ToString()),
                                    ])
                        )),
                    ])
            ]);
}
