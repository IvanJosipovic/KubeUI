using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.RateLimiting;
using KubeUI.AI.Mcp;
using KubeUI.AI.Permissions;
using KubeUI.Avalonia.Infrastructure.Mcp;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Desktop;
using KubeUI.Kubernetes.Serialization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.Mcp;

public sealed class McpServerHostTests
{
    [Fact]
    public void mcp_call_output_limit_rejects_oversized_text_results()
    {
        var oversized = new CallToolResult
        {
            Content = [new TextContentBlock { Text = new string('x', 1024 * 1024 + 1) }]
        };
        var bounded = new CallToolResult
        {
            Content = [new TextContentBlock { Text = "small result" }]
        };

        McpToolAuditFilter.ExceedsOutputLimit(oversized).ShouldBeTrue();
        McpToolAuditFilter.ExceedsOutputLimit(bounded).ShouldBeFalse();

        using var structuredDocument = JsonDocument.Parse($"\"{new string('y', 1024 * 1024)}\"");
        var oversizedStructured = new CallToolResult
        {
            StructuredContent = structuredDocument.RootElement.Clone()
        };
        McpToolAuditFilter.ExceedsOutputLimit(oversizedStructured).ShouldBeTrue();
    }

    [Fact]
    public async Task enabled_mcp_host_rate_limits_each_external_client()
    {
        var port = GetAvailablePort();
        var settings = new Settings { McpServerEnabled = true, McpServerPort = port };
        var settingsService = CreateSettingsService(settings);
        using var host = Program.CreateStartedHost([], includeOptionalServices: false, configureServices: services =>
        {
            services.Replace(ServiceDescriptor.Singleton(settingsService.Object));
        }, mcpPortOverride: port, mcpEnabledOverride: true);
        var credentials = host.Services.GetRequiredService<IMcpCredentialService>();
        var firstToken = credentials.CreateExternalCredential("rate-test", ["dev"]).Token;
        var secondToken = credentials.CreateExternalCredential("other-rate-test", ["dev"]).Token;

        try
        {
            using var client = new HttpClient();
            for (var index = 0; index < McpServerHostedService.MaximumRequestsPerMinute; index++)
            {
                using var request = CreateInitializeRequest(settings);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", firstToken);
                using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
                response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            }

            using var limitedRequest = CreateInitializeRequest(settings);
            limitedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", firstToken);
            using var limited = await client.SendAsync(limitedRequest, TestContext.Current.CancellationToken);
            limited.StatusCode.ShouldBe(System.Net.HttpStatusCode.TooManyRequests);

            using var secondClientRequest = CreateInitializeRequest(settings);
            secondClientRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secondToken);
            using var secondClientResponse = await client.SendAsync(secondClientRequest, TestContext.Current.CancellationToken);
            secondClientResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task enabled_mcp_host_rate_limits_requests_before_authentication()
    {
        var port = GetAvailablePort();
        var settings = new Settings { McpServerEnabled = true, McpServerPort = port };
        using var host = Program.CreateStartedHost([], includeOptionalServices: false,
            mcpPortOverride: port, mcpEnabledOverride: true);

        try
        {
            using var client = new HttpClient();
            for (var index = 0; index < McpServerHostedService.MaximumRequestsPerMinutePerAddress; index++)
            {
                using var request = CreateInitializeRequest(settings);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");
                using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
                response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);
            }

            using var limitedRequest = CreateInitializeRequest(settings);
            limitedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");
            using var limited = await client.SendAsync(limitedRequest, TestContext.Current.CancellationToken);
            limited.StatusCode.ShouldBe(System.Net.HttpStatusCode.TooManyRequests);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task mcp_middleware_ignores_untrusted_forwarded_authority_headers()
    {
        const int port = 62888;
        var credentials = new McpCredentialService(CreateSettingsService(new Settings()).Object);
        var token = credentials.CreateExternalCredential("forwarded-test", ["dev"]).Token;
        var dispatchCount = 0;
        var middleware = new McpSecurityMiddleware(_ =>
        {
            dispatchCount++;
            return Task.CompletedTask;
        }, port);
        var context = CreateAuthorizedContext(port, token);
        context.Request.Headers["X-Forwarded-Host"] = "attacker.example";
        context.Request.Headers["X-Forwarded-Port"] = "443";
        context.Request.Headers["X-Forwarded-Proto"] = "https";

        await middleware.InvokeAsync(context, credentials, new McpRequestIdentityAccessor());

        context.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);
        dispatchCount.ShouldBe(1);
    }

    [Fact]
    public async Task enabled_mcp_host_limits_global_concurrency()
    {
        var port = GetAvailablePort();
        using var host = Program.CreateStartedHost([], includeOptionalServices: false,
            mcpPortOverride: port, mcpEnabledOverride: true);
        var limiter = host.Services.GetRequiredService<IOptions<RateLimiterOptions>>().Value.GlobalLimiter!;
        var context = new DefaultHttpContext { RequestServices = host.Services };
        var permits = new RateLimitLease[McpServerHostedService.MaximumConcurrentRequests];

        try
        {
            for (var index = 0; index < permits.Length; index++)
            {
                permits[index] = limiter.AttemptAcquire(context, 1);
                permits[index].IsAcquired.ShouldBeTrue();
            }

            using var rejected = limiter.AttemptAcquire(context, 1);
            rejected.IsAcquired.ShouldBeFalse();
        }
        finally
        {
            foreach (var permit in permits)
                permit?.Dispose();
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task enabled_mcp_host_rejects_an_untrusted_origin_before_initialization()
    {
        var port = GetAvailablePort();
        var settings = new Settings
        {
            McpServerEnabled = true,
            McpServerPort = port
        };
        using var host = Program.CreateStartedHost([], includeOptionalServices: false,
            mcpPortOverride: port, mcpEnabledOverride: true);

        try
        {
            using var client = new HttpClient();
            using var request = CreateInitializeRequest(settings);
            request.Headers.TryAddWithoutValidation("Origin", "http://attacker.example");

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Forbidden);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task enabled_mcp_host_rejects_an_untrusted_host_before_initialization()
    {
        var port = GetAvailablePort();
        var settings = new Settings
        {
            McpServerEnabled = true,
            McpServerPort = port
        };
        using var host = Program.CreateStartedHost([], includeOptionalServices: false,
            mcpPortOverride: port, mcpEnabledOverride: true);

        try
        {
            using var client = new HttpClient();
            using var request = CreateInitializeRequest(settings);
            request.Headers.Host = $"attacker.example:{port}";

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Theory]
    [InlineData(McpServerConfiguration.Host)]
    [InlineData(McpServerConfiguration.Localhost)]
    public async Task enabled_mcp_host_accepts_an_exact_loopback_origin_with_a_valid_token(string allowedHost)
    {
        var port = GetAvailablePort();
        var settings = new Settings { McpServerEnabled = true, McpServerPort = port };
        var settingsService = CreateSettingsService(settings);
        using var host = Program.CreateStartedHost([], includeOptionalServices: false, configureServices: services =>
        {
            services.Replace(ServiceDescriptor.Singleton(settingsService.Object));
        }, mcpPortOverride: port, mcpEnabledOverride: true);

        try
        {
            using var client = new HttpClient();
            using var request = CreateInitializeRequest(settings);
            request.Headers.Host = $"{allowedHost}:{port}";
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateExternalToken(host));
            request.Headers.TryAddWithoutValidation("Origin", $"http://{allowedHost}:{port}");

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Theory]
    [InlineData("null")]
    [InlineData("http://127.0.0.1:1")]
    [InlineData("https://127.0.0.1:62888")]
    [InlineData("http://127.0.0.1.attacker.example:62888")]
    [InlineData("http://127.0.0.1:62888/")]
    [InlineData("http://user@127.0.0.1:62888")]
    [InlineData("http://127.0.0.1:62888?query=1")]
    [InlineData("http://127.0.0.1:62888#fragment")]
    [InlineData("http://localhost.attacker.example:62888")]
    public async Task enabled_mcp_host_rejects_non_exact_origins(string origin)
    {
        var port = GetAvailablePort();
        var settings = new Settings { McpServerEnabled = true, McpServerPort = port };
        var settingsService = CreateSettingsService(settings);
        using var host = Program.CreateStartedHost([], includeOptionalServices: false, configureServices: services =>
        {
            services.Replace(ServiceDescriptor.Singleton(settingsService.Object));
        }, mcpPortOverride: port, mcpEnabledOverride: true);

        try
        {
            using var client = new HttpClient();
            using var request = CreateInitializeRequest(settings);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateExternalToken(host));
            request.Headers.TryAddWithoutValidation("Origin", origin.Replace("62888", port.ToString(), StringComparison.Ordinal));

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Forbidden);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task enabled_mcp_host_requires_a_bearer_token_before_tool_dispatch()
    {
        var port = GetAvailablePort();
        var settings = new Settings { McpServerEnabled = true, McpServerPort = port };
        var settingsService = CreateSettingsService(settings);
        var navigation = new Mock<IResourceNavigationService>(MockBehavior.Strict);
        using var host = Program.CreateStartedHost([], includeOptionalServices: false, configureServices: services =>
        {
            services.Replace(ServiceDescriptor.Singleton(settingsService.Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IClusterRuntimeCatalog>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IMcpClusterSession>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IKubernetesYamlSerializer>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IAgentPermissionService>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(navigation.Object));
        }, mcpPortOverride: port, mcpEnabledOverride: true);

        try
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, McpServerConfiguration.GetEndpoint(settings));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
            request.Content = JsonContent.Create(new
            {
                jsonrpc = "2.0",
                id = 1,
                method = "tools/call",
                @params = new
                {
                    name = "kubeui_open_resource_list",
                    arguments = new { cluster = "dev", apiVersion = "apps/v1", kind = "Deployment" }
                }
            });

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);
            navigation.VerifyNoOtherCalls();
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Theory]
    [InlineData("POST", "/mcp")]
    [InlineData("GET", "/mcp")]
    [InlineData("DELETE", "/mcp")]
    [InlineData("OPTIONS", "/mcp")]
    [InlineData("POST", "/mcp/")]
    public async Task enabled_mcp_host_requires_authentication_before_method_or_route_dispatch(
        string method, string path)
    {
        var port = GetAvailablePort();
        var settings = new Settings { McpServerEnabled = true, McpServerPort = port };
        using var host = Program.CreateStartedHost([], includeOptionalServices: false, configureServices: services =>
        {
            services.Replace(ServiceDescriptor.Singleton(CreateSettingsService(settings).Object));
        }, mcpPortOverride: port, mcpEnabledOverride: true);

        try
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod(method), $"http://127.0.0.1:{port}{path}");

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task enabled_mcp_host_rejects_multiple_origin_values()
    {
        var port = GetAvailablePort();
        var settings = new Settings { McpServerEnabled = true, McpServerPort = port };
        using var host = Program.CreateStartedHost([], includeOptionalServices: false, configureServices: services =>
        {
            services.Replace(ServiceDescriptor.Singleton(CreateSettingsService(settings).Object));
        }, mcpPortOverride: port, mcpEnabledOverride: true);

        try
        {
            using var client = new HttpClient();
            using var request = CreateInitializeRequest(settings);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateExternalToken(host));
            request.Headers.TryAddWithoutValidation("Origin", $"http://127.0.0.1:{port}");
            request.Headers.TryAddWithoutValidation("Origin", "http://attacker.example");

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Forbidden);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task enabled_mcp_host_rejects_a_revoked_bearer_token()
    {
        var port = GetAvailablePort();
        var settings = new Settings { McpServerEnabled = true, McpServerPort = port };
        using var host = Program.CreateStartedHost([], includeOptionalServices: false, configureServices: services =>
        {
            services.Replace(ServiceDescriptor.Singleton(CreateSettingsService(settings).Object));
        }, mcpPortOverride: port, mcpEnabledOverride: true);
        var credentials = host.Services.GetRequiredService<IMcpCredentialService>();
        var token = credentials.CreateExternalCredential("revoked-test", ["dev"]).Token;
        var grant = settings.McpExternalClientGrants.Single();
        credentials.RevokeExternalCredential(grant.Id).ShouldBeTrue();

        try
        {
            using var client = new HttpClient();
            using var request = CreateInitializeRequest(settings);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task enabled_mcp_host_rejects_request_bodies_over_one_mibibyte()
    {
        var port = GetAvailablePort();
        var settings = new Settings { McpServerEnabled = true, McpServerPort = port };
        using var host = Program.CreateStartedHost([], includeOptionalServices: false, configureServices: services =>
        {
            services.Replace(ServiceDescriptor.Singleton(CreateSettingsService(settings).Object));
        }, mcpPortOverride: port, mcpEnabledOverride: true);

        try
        {
            using var client = new HttpClient();
            using var request = CreateInitializeRequest(settings);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateExternalToken(host));
            request.Content = new StringContent(new string('x', 1024 * 1024 + 1), System.Text.Encoding.UTF8, "application/json");

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.RequestEntityTooLarge);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task enabled_mcp_host_accepts_initialize_and_stops_cleanly()
    {
        var port = GetAvailablePort();
        var settings = new Settings
        {
            McpServerEnabled = true,
            McpServerPort = port
        };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        settingsService.Setup(service => service.TrySaveSettings()).Returns(true);
        using var host = Program.CreateStartedHost([], includeOptionalServices: false, configureServices: services =>
        {
            services.Replace(ServiceDescriptor.Singleton(settingsService.Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IClusterRuntimeCatalog>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IMcpClusterSession>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IKubernetesYamlSerializer>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IAgentPermissionService>(MockBehavior.Strict).Object));
        }, mcpPortOverride: port, mcpEnabledOverride: true);

        try
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, McpServerConfiguration.GetEndpoint(settings));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateExternalToken(host));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
            request.Headers.Add("MCP-Protocol-Version", "2025-06-18");
            request.Content = JsonContent.Create(new
            {
                jsonrpc = "2.0",
                id = 1,
                method = "initialize",
                @params = new
                {
                    protocolVersion = "2025-06-18",
                    capabilities = new { },
                    clientInfo = new { name = "KubeUI.Tests", version = "1.0" }
                }
            });

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                TestContext.Current.CancellationToken);
            response.IsSuccessStatusCode.ShouldBeTrue();
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Theory]
    [InlineData("resources/list", "resources")]
    [InlineData("resources/templates/list", "resourceTemplates")]
    public async Task enabled_mcp_host_returns_empty_resource_catalog_for_discovery(
        string method,
        string resultProperty)
    {
        var port = GetAvailablePort();
        var settings = new Settings
        {
            McpServerEnabled = true,
            McpServerPort = port
        };
        var settingsService = CreateSettingsService(settings);
        using var host = Program.CreateStartedHost([], includeOptionalServices: false, configureServices: services =>
        {
            services.Replace(ServiceDescriptor.Singleton(settingsService.Object));
        }, mcpPortOverride: port, mcpEnabledOverride: true);

        try
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, McpServerConfiguration.GetEndpoint(settings));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateExternalToken(host));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
            request.Headers.Add("MCP-Protocol-Version", "2025-06-18");
            request.Content = JsonContent.Create(new
            {
                jsonrpc = "2.0",
                id = 3,
                method,
                @params = new { }
            });

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                TestContext.Current.CancellationToken);
            response.IsSuccessStatusCode.ShouldBeTrue();

            using var document = JsonDocument.Parse(await ReadFirstEventDataAsync(response));
            document.RootElement.GetProperty("result").GetProperty(resultProperty)
                .GetArrayLength().ShouldBe(0);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task enabled_mcp_host_invokes_registered_tools()
    {
        var port = GetAvailablePort();
        var settings = new Settings
        {
            McpServerEnabled = true,
            McpServerPort = port
        };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        settingsService.Setup(service => service.TrySaveSettings()).Returns(true);
        var clusterCatalog = new Mock<IClusterRuntimeCatalog>(MockBehavior.Strict);
        var navigation = new Mock<IResourceNavigationService>(MockBehavior.Strict);
        navigation.Setup(service => service.OpenResourceListAsync("dev", "apps/v1", "Deployment"))
            .ReturnsAsync(true);
        clusterCatalog.Setup(catalog => catalog.GetDefault()).Returns((IClusterRuntime?)null);
        clusterCatalog.SetupGet(catalog => catalog.Clusters).Returns([]);
        using var host = Program.CreateStartedHost([], includeOptionalServices: false, configureServices: services =>
        {
            services.Replace(ServiceDescriptor.Singleton(settingsService.Object));
            services.Replace(ServiceDescriptor.Singleton(clusterCatalog.Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IMcpClusterSession>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IKubernetesYamlSerializer>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IAgentPermissionService>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(navigation.Object));
        }, mcpPortOverride: port, mcpEnabledOverride: true);

        try
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, McpServerConfiguration.GetEndpoint(settings));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateExternalToken(host));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
            request.Headers.Add("MCP-Protocol-Version", "2025-06-18");
            request.Content = JsonContent.Create(new
            {
                jsonrpc = "2.0",
                id = 2,
                method = "tools/call",
                @params = new
                {
                    name = "kubeui_open_resource_list",
                    arguments = new { cluster = "dev", apiVersion = "apps/v1", kind = "Deployment" }
                }
            });

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                TestContext.Current.CancellationToken);
            response.IsSuccessStatusCode.ShouldBeTrue();
            var body = await ReadFirstEventDataAsync(response);
            body.ShouldNotContain("\"isError\":true");
            body.ShouldContain("true");
            navigation.VerifyAll();
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task desktop_host_continues_when_static_mcp_port_cannot_be_bound()
    {
        using var blocker = new TcpListener(System.Net.IPAddress.Loopback, 0);
        blocker.Start();
        var blockedPort = ((System.Net.IPEndPoint)blocker.LocalEndpoint).Port;

        using var host = Program.CreateStartedHost([], includeOptionalServices: false, configureServices: services =>
        {
            services.Replace(ServiceDescriptor.Singleton(new Mock<IClusterRuntimeCatalog>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IMcpClusterSession>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IKubernetesYamlSerializer>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IAgentPermissionService>(MockBehavior.Strict).Object));
        }, mcpPortOverride: blockedPort, mcpEnabledOverride: true);

        try
        {
            host.Services.GetRequiredService<IMcpServerState>().BoundPort.ShouldBeNull();
            host.Services.GetService<IServer>().ShouldBeNull();
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task host_startup_records_the_bound_port_when_the_configured_port_is_available()
    {
        var port = GetAvailablePort();
        var settings = new Settings { McpServerEnabled = true, McpServerPort = port };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        settingsService.Setup(service => service.TrySaveSettings()).Returns(true);
        using var application = Program.CreateStartedHost([], includeOptionalServices: false, configureServices: services =>
        {
            services.Replace(ServiceDescriptor.Singleton(settingsService.Object));
        }, mcpPortOverride: port, mcpEnabledOverride: true);

        try
        {
            application.Services.GetRequiredService<IMcpServerState>()
                .BoundPort.ShouldBe(port);
        }
        finally
        {
            await application.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task disposing_the_mcp_host_releases_its_bound_port()
    {
        var port = GetAvailablePort();
        using var host = Program.CreateStartedHost([], includeOptionalServices: false,
            mcpPortOverride: port, mcpEnabledOverride: true);
        var hostedService = host.Services.GetServices<IHostedService>()
            .Single(service => service.GetType().Name == "McpServerHostedService");

        await ((IAsyncDisposable)hostedService).DisposeAsync();

        host.Services.GetRequiredService<IMcpServerState>().BoundPort.ShouldBeNull();
    }

    [Fact]
    public async Task desktop_host_serves_mcp_from_its_single_host()
    {
        var port = GetAvailablePort();
        var settings = new Settings
        {
            McpServerEnabled = true,
            McpServerPort = port
        };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        settingsService.Setup(service => service.TrySaveSettings()).Returns(true);
        using var host = Program.CreateStartedHost([], includeOptionalServices: false, configureServices: services =>
        {
            services.Replace(ServiceDescriptor.Singleton(settingsService.Object));
        }, mcpPortOverride: port, mcpEnabledOverride: true);
        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, McpServerConfiguration.GetEndpoint(settings));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateExternalToken(host));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        request.Headers.Add("MCP-Protocol-Version", "2025-06-18");
        request.Content = JsonContent.Create(new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "initialize",
            @params = new
            {
                protocolVersion = "2025-06-18",
                capabilities = new { },
                clientInfo = new { name = "KubeUI.Tests", version = "1.0" }
            }
        });

        try
        {
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                TestContext.Current.CancellationToken);
            response.IsSuccessStatusCode.ShouldBeTrue();
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task disabled_mcp_host_does_not_claim_a_fixed_default_port()
    {
        using var application = Program.CreateStartedHost([], includeOptionalServices: false, mcpEnabledOverride: false);

        try
        {
            application.Services.GetService<IServer>().ShouldBeNull();
        }
        finally
        {
            await application.StopAsync(CancellationToken.None);
        }
    }

    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        return ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static Mock<ISettingsService> CreateSettingsService(Settings settings)
    {
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        settingsService.Setup(service => service.TrySaveSettings()).Returns(true);
        return settingsService;
    }

    private static DefaultHttpContext CreateAuthorizedContext(int port, string token)
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString(McpServerConfiguration.Host, port);
        context.Request.Headers.Authorization = $"Bearer {token}";
        return context;
    }

    private static string CreateExternalToken(IHost host)
        => host.Services.GetRequiredService<IMcpCredentialService>()
            .CreateExternalCredential("host-test", ["dev", "test"]).Token;

    private static async Task<string> ReadFirstEventDataAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        using var reader = new StreamReader(stream);
        while (await reader.ReadLineAsync(TestContext.Current.CancellationToken) is { } line)
        {
            if (line.StartsWith("data: ", StringComparison.Ordinal))
                return line[6..];
        }
        throw new InvalidOperationException("The MCP response did not contain an event payload.");
    }

    private static HttpRequestMessage CreateInitializeRequest(Settings settings)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, McpServerConfiguration.GetEndpoint(settings));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        request.Headers.Add("MCP-Protocol-Version", "2025-06-18");
        request.Content = JsonContent.Create(new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "initialize",
            @params = new
            {
                protocolVersion = "2025-06-18",
                capabilities = new { },
                clientInfo = new { name = "KubeUI.Tests", version = "1.0" }
            }
        });
        return request;
    }
}
