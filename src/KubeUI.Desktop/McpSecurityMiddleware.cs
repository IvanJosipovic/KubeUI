using KubeUI.AI.Mcp;
using KubeUI.Avalonia.Infrastructure.Mcp;
using Microsoft.AspNetCore.Http;

namespace KubeUI.Desktop;

internal sealed class McpSecurityMiddleware(RequestDelegate next, int port)
{
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
        if (!IsAllowedOrigin(context.Request.Headers.Origin))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        if (!TryGetBearerToken(context.Request.Headers.Authorization, out var token)
            || credentials.Authenticate(token) is not { } identity)
        {
            context.Response.Headers.WWWAuthenticate = "Bearer";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        using (identityAccessor.Enter(identity))
            await _next(context).ConfigureAwait(false);
    }

    private bool IsAllowedOrigin(Microsoft.Extensions.Primitives.StringValues values)
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
            && uri.Port == _port
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
