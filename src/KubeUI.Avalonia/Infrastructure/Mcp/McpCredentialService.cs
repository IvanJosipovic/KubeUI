using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using KubeUI.AI.Mcp;
using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Services.Settings;

namespace KubeUI.Avalonia.Infrastructure.Mcp;

/// <summary>Issues, validates, and revokes credentials for KubeUI's local MCP endpoint.</summary>
public interface IMcpCredentialService
{
    /// <summary>Creates a persistent credential for an external host client.</summary>
    /// <param name="name">Display name for the client.</param>
    /// <param name="allowedClusters">Exact cluster names to allow, or empty to allow all clusters.</param>
    McpCreatedExternalCredential CreateExternalCredential(string name, IReadOnlyCollection<string> allowedClusters);

    /// <summary>Revokes a persistent external-client credential by identifier.</summary>
    bool RevokeExternalCredential(string id);

    /// <summary>Creates an in-memory credential for one KubeUI-managed ACP session.</summary>
    McpSessionCredential CreateSessionCredential();

    /// <summary>Finds the identity associated with a bearer token, or null when it is invalid or revoked.</summary>
    McpClientIdentity? Authenticate(string token);
}

/// <summary>New external-client credential; token is returned once and never persisted.</summary>
public sealed record McpCreatedExternalCredential(McpExternalClientGrant Grant, string Token)
{
    /// <inheritdoc />
    public override string ToString() => "MCP external credential [token redacted]";
}

/// <summary>Bearer credential lease revoked when its ACP session is disposed.</summary>
public sealed class McpSessionCredential : IDisposable
{
    private readonly Action<string> _revoke;
    private int _disposed;

    internal McpSessionCredential(string token, Action<string> revoke)
    {
        Token = token;
        _revoke = revoke;
    }

    /// <summary>Gets token value for the authenticated MCP transport header.</summary>
    public string Token { get; }

    /// <summary>Revokes this credential from the in-memory verifier set.</summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;
        _revoke(Token);
    }

    /// <inheritdoc />
    public override string ToString() => "MCP session credential [redacted]";
}

/// <inheritdoc />
public sealed class McpCredentialService(ISettingsService settingsService) : IMcpCredentialService
{
    private const int TokenByteCount = 32;
    private const int MaximumNameLength = 80;
    private const int MaximumExternalCredentials = 100;
    private const int MaximumSessionCredentials = 128;
    private const int MaximumClusterCount = 100;
    private const int MaximumClusterNameLength = 128;
    private readonly Lock _settingsLock = new();
    private readonly Lock _sessionLock = new();
    private readonly ISettingsService _settingsService = settingsService;
    private readonly ConcurrentDictionary<string, McpClientIdentity> _sessionCredentials = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public McpCreatedExternalCredential CreateExternalCredential(string name, IReadOnlyCollection<string> allowedClusters)
    {
        ArgumentNullException.ThrowIfNull(allowedClusters);
        var normalizedName = name?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName) || normalizedName.Length > MaximumNameLength)
            throw new ArgumentException($"Client name must contain 1 to {MaximumNameLength} characters.", nameof(name));

        var clusters = allowedClusters
            .Select(cluster => cluster?.Trim())
            .Where(cluster => !string.IsNullOrWhiteSpace(cluster))
            .Select(cluster => cluster!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (clusters.Length > MaximumClusterCount
            || clusters.Any(cluster => cluster.Length > MaximumClusterNameLength))
            throw new ArgumentException("Choose no more than 100 valid cluster names. An empty selection grants access to all clusters.", nameof(allowedClusters));

        var token = GenerateToken();
        var grant = new McpExternalClientGrant(
            Guid.NewGuid().ToString("N"), normalizedName, GetVerifier(token), DateTimeOffset.UtcNow, clusters);
        lock (_settingsLock)
        {
            var grants = _settingsService.Settings.McpExternalClientGrants;
            if (grants.Any(grant => string.Equals(
                grant.Name.Trim(), normalizedName, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("An external MCP client with this name already exists.", nameof(name));

            if (grants.Count >= MaximumExternalCredentials)
                throw new InvalidOperationException($"KubeUI supports up to {MaximumExternalCredentials} external MCP clients.");
            grants.Add(grant);
            if (!_settingsService.TrySaveSettings())
            {
                grants.Remove(grant);
                throw new InvalidOperationException("KubeUI could not securely save the MCP credential.");
            }
        }
        return new McpCreatedExternalCredential(grant, token);
    }

    /// <inheritdoc />
    public bool RevokeExternalCredential(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        lock (_settingsLock)
        {
            var grants = _settingsService.Settings.McpExternalClientGrants;
            var index = -1;
            for (var candidateIndex = 0; candidateIndex < grants.Count; candidateIndex++)
            {
                if (!string.Equals(grants[candidateIndex].Id, id, StringComparison.Ordinal))
                    continue;
                index = candidateIndex;
                break;
            }
            if (index < 0)
                return false;
            var grant = grants[index];
            grants.Remove(grant);
            if (!_settingsService.TrySaveSettings())
            {
                grants.Insert(index, grant);
                throw new InvalidOperationException("KubeUI could not securely save the MCP credential revocation.");
            }
            return true;
        }
    }

    /// <inheritdoc />
    public McpSessionCredential CreateSessionCredential()
    {
        lock (_sessionLock)
        {
            if (_sessionCredentials.Count >= MaximumSessionCredentials)
                throw new InvalidOperationException("Too many active KubeUI MCP sessions.");
            var token = GenerateToken();
            if (!_sessionCredentials.TryAdd(GetVerifier(token), new McpClientIdentity(
                Guid.NewGuid().ToString("N"), "KubeUI ACP session", true, new HashSet<string>(StringComparer.Ordinal))))
                throw new InvalidOperationException("Unable to create a unique KubeUI MCP session credential.");
            return new McpSessionCredential(token, RevokeSessionCredential);
        }
    }

    /// <inheritdoc />
    public McpClientIdentity? Authenticate(string token)
    {
        if (string.IsNullOrEmpty(token) || token.Length > 128)
            return null;

        var verifier = GetVerifier(token);
        if (_sessionCredentials.TryGetValue(verifier, out var sessionIdentity))
            return sessionIdentity;

        lock (_settingsLock)
        {
            var grants = _settingsService.Settings.McpExternalClientGrants;
            var supplied = Convert.FromHexString(verifier);
            foreach (var grant in grants)
            {
                if (!IsVerifierMatch(grant.TokenVerifier, supplied))
                    continue;
                return new McpClientIdentity(
                    grant.Id,
                    grant.Name,
                    false,
                    new HashSet<string>(grant.AllowedClusters, StringComparer.Ordinal));
            }
        }
        return null;
    }

    private static bool IsVerifierMatch(string verifier, byte[] supplied)
    {
        try
        {
            var candidate = Convert.FromHexString(verifier);
            return candidate.Length == supplied.Length && CryptographicOperations.FixedTimeEquals(candidate, supplied);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private void RevokeSessionCredential(string token)
    {
        lock (_sessionLock)
            _sessionCredentials.TryRemove(GetVerifier(token), out _);
    }

    private static string GenerateToken()
    {
        Span<byte> bytes = stackalloc byte[TokenByteCount];
        RandomNumberGenerator.Fill(bytes);
        return $"kui_{Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')}";
    }

    private static string GetVerifier(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
