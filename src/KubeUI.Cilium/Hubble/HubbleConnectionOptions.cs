namespace KubeUI.Cilium.Hubble;

/// <summary>Configures access to a Hubble Relay service.</summary>
public sealed class HubbleConnectionOptions
{
    /// <summary>Gets or sets the namespace containing Hubble Relay.</summary>
    public string Namespace { get; set; } = "kube-system";

    /// <summary>Gets or sets the Hubble Relay Service name.</summary>
    public string ServiceName { get; set; } = "hubble-relay";

    /// <summary>Gets or sets an explicit Service port; null discovers the gRPC port.</summary>
    public int? ServicePort { get; set; }

    /// <summary>Gets or sets whether the local gRPC channel uses TLS; null follows the Service port.</summary>
    public bool? UseTls { get; set; }

    /// <summary>Gets or sets the expected TLS server name when the relay certificate uses another host name.</summary>
    public string? TlsServerName { get; set; } = "ui.hubble-relay.cilium.io";

    /// <summary>Gets or sets a PEM root certificate for a private relay CA.</summary>
    public string? TrustedRootCertificatePem { get; set; }

    /// <summary>Gets or sets an optional PEM client certificate for mutual TLS.</summary>
    public string? ClientCertificatePem { get; set; }

    /// <summary>Gets or sets the PEM private key paired with the client certificate.</summary>
    public string? ClientPrivateKeyPem { get; set; }
}
