using KubeUI.AI.Mcp;
using KubeUI.Avalonia.Infrastructure.Mcp;
using Microsoft.AspNetCore.Http;

namespace KubeUI.Desktop;

internal sealed class McpSecurityMiddleware(RequestDelegate next, int port)
{
    private const string RequestIdentityItemKey = "KubeUI.Mcp.RequestIdentity";
    private readonly RequestDelegate _next = next;
    private readonly int _port = port;

    public async Task InvokeAsync(
        HttpContext context,
        IMcpCredentialService credentials,
        McpRequestIdentityAccessor identityAccessor)
    {
        // HostFiltering checks the allowed names; this keeps the authority tied to the bound port.
        if (context.Request.Host.Port != _port)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        // Origin checks also apply to non-browser callers; CORS alone cannot enforce this boundary.
        if (!IsAllowedOrigin(context.Request.Headers.Origin, _port))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        if (GetRequestIdentity(context, credentials, _port) is not { } identity)
        {
            context.Response.Headers.WWWAuthenticate = "Bearer";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        using (identityAccessor.Enter(identity))
            await _next(context).ConfigureAwait(false);
    }

    internal static McpClientIdentity? GetRequestIdentity(
        HttpContext context,
        IMcpCredentialService credentials,
        int port)
    {
        if (context.Items.TryGetValue(RequestIdentityItemKey, out var cachedIdentity))
            return cachedIdentity as McpClientIdentity;

        McpClientIdentity? identity = null;
        if (context.Request.Host.Port == port
            && IsAllowedOrigin(context.Request.Headers.Origin, port)
            && TryGetBearerToken(context.Request.Headers.Authorization, out var token))
        {
            identity = credentials.Authenticate(token);
        }

        // Cache both successful and failed authentication so the limiter chain and security
        // middleware make one consistent decision for this request.
        context.Items[RequestIdentityItemKey] = identity;
        return identity;
    }

    private static bool IsAllowedOrigin(Microsoft.Extensions.Primitives.StringValues values, int port)
    {
        if (values.Count == 0)
            return true;
        if (values.Count != 1)
            return false;

        var origin = values[0];
        if (string.IsNullOrWhiteSpace(origin) || origin.Contains(',')
            || !Uri.TryCreate(origin, UriKind.Absolute, out var uri))
            return false;

        return string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.Ordinal)
            && uri.Port == port
            && (string.Equals(uri.Host, McpServerConfiguration.Host, StringComparison.OrdinalIgnoreCase)
                || string.Equals(uri.Host, McpServerConfiguration.Localhost, StringComparison.OrdinalIgnoreCase))
            && string.IsNullOrEmpty(uri.UserInfo)
            && uri.AbsolutePath == "/"
            && string.IsNullOrEmpty(uri.Query)
            && string.IsNullOrEmpty(uri.Fragment)
            && !origin.EndsWith('/');
    }

    private static bool TryGetBearerToken(Microsoft.Extensions.Primitives.StringValues values, out string token)
    {
        token = string.Empty;
        if (values.Count != 1 || values[0] is not { } header)
            return false;
        const string prefix = "Bearer ";
        if (!header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;
        token = header[prefix.Length..].Trim();
        return token.Length is > 0 and <= 128 && !token.Contains(' ');
    }
}
