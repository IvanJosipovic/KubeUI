using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Security.Cryptography;
using Avalonia.Controls.Templates;

namespace KubeUI.Views;

public partial class CertificateItem : ViewBase
{
    public static readonly DirectProperty<CertificateItem, byte[]> BytesProperty =
    AvaloniaProperty.RegisterDirect<CertificateItem, byte[]>(
    nameof(Bytes),
    o => o.Bytes,
    (o, v) => o.Bytes = v);

    private byte[] _bytes;

    public byte[] Bytes
    {
        get { return _bytes; }
        set { SetAndRaise(BytesProperty, ref _bytes, value); Convert(); }
    }

    private readonly ObservableCollection<X509Certificate2> _certificates = [];
    private readonly ObservableCollection<RSA> _rsa = [];
    private readonly ObservableCollection<ECDsa> _ecdsa = [];

    private void Convert()
    {
        _certificates.Clear();
        _rsa.Clear();
        _ecdsa.Clear();
        var text =  Encoding.UTF8.GetString(Bytes);
        Span<byte> buffer = stackalloc byte[Encoding.UTF8.GetByteCount(text)];
        byte[] bytes = Bytes;

        if (System.Convert.TryFromBase64String(text, buffer, out var decodedBytes))
        {
            text = Encoding.UTF8.GetString(buffer);
            bytes = Encoding.UTF8.GetBytes(text);
        }

        if (text.StartsWith("-----BEGIN CERTIFICATE-----"))
        {
            try
            {
                var cert = new X509Certificate2(bytes);
                _certificates.Add(cert);
            }
            catch (Exception ex){}
        }
        else if (text.StartsWith("-----BEGIN RSA PRIVATE KEY-----"))
        {
            try
            {
                RSA key = RSA.Create();
                key.ImportRSAPrivateKey(bytes, out _);

                _rsa.Add(key);
            }
            catch (Exception ex){}
        }
        else if (text.StartsWith("-----BEGIN PRIVATE KEY-----"))
        {
            try
            {
                RSA key = RSA.Create();
                key.ImportPkcs8PrivateKey(bytes, out _);

                _rsa.Add(key);
            }
            catch (Exception ex){}
        }
        else if (text.StartsWith("-----BEGIN RSA PUBLIC KEY-----"))
        {
            try
            {
                RSA key = RSA.Create();
                key.ImportRSAPublicKey(bytes, out _);

                _rsa.Add(key);
            }
            catch (Exception ex) { }
        }
        else if (text.StartsWith("-----BEGIN PUBLIC KEY-----"))
        {
            try
            {
                RSA key = RSA.Create();
                key.ImportSubjectPublicKeyInfo(bytes, out _);

                _rsa.Add(key);
            }
            catch (Exception ex) { }
        }
        else if (text.StartsWith("-----BEGIN EC PRIVATE KEY-----"))
        {
            try
            {
                var ecdsa = ECDsa.Create();
                ecdsa.ImportECPrivateKey(bytes, out _);

                _ecdsa.Add(ecdsa);
            }
            catch (Exception ex) { }
        }
    }

    protected override object Build() =>
        new StackPanel()
            .Children([
                new ItemsControl()
                    .ItemsSource(@_certificates)
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
                    .ItemsSource(@_rsa)
                    .ItemTemplate(new FuncDataTemplate<RSA>((x,_) =>
                        new StackPanel()
                            .Children([
                                new PropertyItem().Key("Key Exchange Algorithm").Value(x.KeyExchangeAlgorithm),
                                new PropertyItem().Key("Signature Algorithm").Value(x.SignatureAlgorithm),
                                new PropertyItem().Key("Key Size").Value(x.KeySize.ToString()),
                            ])
                )),
                new ItemsControl()
                    .ItemsSource(@_ecdsa)
                    .ItemTemplate(new FuncDataTemplate<ECDsa>((x,_) =>
                        new StackPanel()
                            .Children([
                                new PropertyItem().Key("Key Exchange Algorithm").Value(x.KeyExchangeAlgorithm),
                                new PropertyItem().Key("Signature Algorithm").Value(x.SignatureAlgorithm),
                                new PropertyItem().Key("Key Size").Value(x.KeySize.ToString()),
                            ])
                )),
                ]);
}
