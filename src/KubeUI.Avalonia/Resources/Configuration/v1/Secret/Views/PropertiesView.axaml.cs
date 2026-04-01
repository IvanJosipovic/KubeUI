using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using k8s.Models;

namespace KubeUI.Avalonia.Resources.Configuration.v1.Secret.Views;

public partial class PropertiesView : UserControl
{
    public PropertiesView()
    {
        InitializeComponent();
#if DEBUG
        if (Design.IsDesignMode)
        {
            DataContext = new V1Secret()
            {
                ApiVersion = V1Secret.KubeApiVersion,
                Kind = V1Secret.KubeKind,
                Metadata = new()
                {
                    Name = "testSecret",
                    NamespaceProperty = "default"
                },
                Data = new Dictionary<string, byte[]>(
                [
                    new KeyValuePair<string,byte[]>("testKey", Encoding.UTF8.GetBytes("test")),
                    new KeyValuePair<string,byte[]>("ca.crt", Encoding.UTF8.GetBytes("""
                        -----BEGIN CERTIFICATE-----
                        MIIDDzCCAfegAwIBAgIUS/nDwUin7yOfXmpyjoOAK3PfW98wDQYJKoZIhvcNAQEL
                        BQAwFzEVMBMGA1UEAwwMMTAuMTUyLjE4My4xMB4XDTIxMTExODA1Mjg1M1oXDTMx
                        MTExNjA1Mjg1M1owFzEVMBMGA1UEAwwMMTAuMTUyLjE4My4xMIIBIjANBgkqhkiG
                        9w0BAQEFAAOCAQ8AMIIBCgKCAQEAxhcLDs54S6Xfjs2ir/1gNrqfW74TUUL1ucVg
                        5tf80pI7Y5dnaAXUIyf6VS5xyqNqDskqEAeOukK6ti1VeBut+BdoHwzyBZ6z/dcE
                        /9Vt9qXgMi12LoVvAs0wUGq9y5oDeeaQkXZPXaV1O7JDOH9zixiWaVNfH+i+Wrhr
                        oN3bTHk22KbmpXROIl5Ooc2hFqLGSLT+E3XhKQt/rve4mtkADTjjbwSI34h5x3OZ
                        fPueOS31hHoZSdaUAvQ9e4AaEasJSWhVTlBUe645AZFZk1/I2HT0e19Jif06P0lS
                        qvXzHByuSuCHGDJhqIie2dfNJVD+kpB6XpwgY3FjP2e736QvawIDAQABo1MwUTAd
                        BgNVHQ4EFgQUynvMpHT4K6VML/rsN8xu76Voy2cwHwYDVR0jBBgwFoAUynvMpHT4
                        K6VML/rsN8xu76Voy2cwDwYDVR0TAQH/BAUwAwEB/zANBgkqhkiG9w0BAQsFAAOC
                        AQEAsK97dYV1mik5s3aD0EPAeyGhON2QZmT4kmuw5G4prc+RdRiZFWB3ix8KCZcy
                        ofDnlqei3U2lQRxnFmuHh69r3UDDocGTQAjUiiT3Z9bnKYFUZFo4/L+X2z3Lc+eE
                        JlIpK4hAxRnKub/2MfFp6XSIp1+9cIxmO2Bq1YTCQ6+KXaX6P5ek1+URiOTunXeG
                        ZNrkxAlzNKJ6dV1f/qptXB4OtC2MN0bRVUadvZwg0SMukydvaUWe9zzn/VomschE
                        /E8kRFoRnYwzVtLK8lplGWhudrxwRdOriU0JdYFVunOUdAY3Q4LCBGUZA3F8UXpM
                        faewOFkMRa+eSx/pNEXKh3dLug==
                        -----END CERTIFICATE-----
                        """)),
                    new KeyValuePair<string, byte[]>("token", Encoding.UTF8.GetBytes("eyJhbGciOiJSUzI1NiIsImtpZCI6IjBtSkQ4dGRrTzM0REF6NVpDOXJLYXlYbWhXcE1acktaQWMzMW0xTDktZU0ifQ.eyJpc3MiOiJrdWJlcm5ldGVzL3NlcnZpY2VhY2NvdW50Iiwia3ViZXJuZXRlcy5pby9zZXJ2aWNlYWNjb3VudC9uYW1lc3BhY2UiOiJkZWZhdWx0Iiwia3ViZXJuZXRlcy5pby9zZXJ2aWNlYWNjb3VudC9zZWNyZXQubmFtZSI6ImRlZmF1bHQtdG9rZW4tZ3ZzN2siLCJrdWJlcm5ldGVzLmlvL3NlcnZpY2VhY2NvdW50L3NlcnZpY2UtYWNjb3VudC5uYW1lIjoiZGVmYXVsdCIsImt1YmVybmV0ZXMuaW8vc2VydmljZWFjY291bnQvc2VydmljZS1hY2NvdW50LnVpZCI6ImZlODYzOWJmLTU4NmYtNGI2OC1iM2YxLTE4NjFlODA1Y2EzMyIsInN1YiI6InN5c3RlbTpzZXJ2aWNlYWNjb3VudDpkZWZhdWx0OmRlZmF1bHQifQ.br_gm5jwFtljktbk_M-6WaMVFXByVBaRbVXZz25pG_eu3biNcakWEw919BEFxrVR4Mbx6BMlWIty53eM-xaJokQ_XhIHfRoTwMwzKxE6RvRg1g4GFvHLLPSWuqS658z6BbDkJAzS9dp5v-gmp1_VDwP35uZrzY1LlayPAKrQja93-CFafnJvSha7KgOvYR7-qIhYeLSXFC4fAcBpbqy7jEoi3FIo-tdiDDg5fFInb0_cosv0tguU4CqtaDigq06DfvIT79qEb7kZ0lOBGpdAVOwUKJsyv598Amxz49igIA9HGUFYduaL42t6R7nni94kzZzomVU7RiiUMItFaicaDQ"))
                ])
            };
        }
#endif
    }
}

