using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Channels;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Testing.Kubernetes.Transport;

public sealed class FakeKubernetesHttpApi : DelegatingHandler
{
    private readonly BackendState _state;
    private readonly ConcurrentDictionary<string, ResourceDefinition> _definitions;
    private readonly ConcurrentDictionary<string, JsonObject> _resources;
    private readonly ConcurrentDictionary<string, ConcurrentBag<Channel<byte[]>>> _watchers;
    private readonly CancellationTokenSource _shutdownCancellation;
    public Task<HttpResponseMessage> HandleAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        => SendAsync(request, cancellationToken);
    private readonly ConcurrentDictionary<string, bool> _permissions = new(StringComparer.Ordinal);

    public FakeKubernetesHttpApi()
        : this(new BackendState(), initializeDefaults: true)
    {
    }

    private FakeKubernetesHttpApi(BackendState state, bool initializeDefaults = false)
    {
        _state = state;
        _definitions = state.Definitions;
        _resources = state.Resources;
        _watchers = state.Watchers;
        _shutdownCancellation = state.ShutdownCancellation;

        if (!initializeDefaults)
            return;

        Add(new V1Namespace { Metadata = new V1ObjectMeta { Name = "default" } });
        Add(new V1Node { Metadata = new V1ObjectMeta { Name = "node-1" } });
        Add(new V1Secret
        {
            Metadata = new V1ObjectMeta { Name = "my-serviceaccount", NamespaceProperty = "my-app" },
            Data = new Dictionary<string, byte[]> { ["token"] = Encoding.UTF8.GetBytes("fake-token") },
        });
    }

    internal FakeKubernetesHttpApi CreateClient(
        string? authenticatedUser = null,
        bool? useRoleBasedAuthorization = null)
    {
        return new FakeKubernetesHttpApi(_state)
        {
            AuthenticatedUser = authenticatedUser ?? AuthenticatedUser,
            UseRoleBasedAuthorization = useRoleBasedAuthorization ?? UseRoleBasedAuthorization,
        };
    }

    public bool UseRoleBasedAuthorization { get; set; }

    public bool RequireAuthorizationForDiscovery
    {
        get => Volatile.Read(ref _state.RequireAuthorizationForDiscovery) != 0;
        set => Volatile.Write(ref _state.RequireAuthorizationForDiscovery, value ? 1 : 0);
    }

    /// <summary>Gets or sets the ETag used for both core and grouped discovery responses.</summary>
    public string DiscoveryETag
    {
        get
        {
            lock (_state.DiscoveryGate)
            {
                return _state.CoreDiscoveryETag;
            }
        }
        set
        {
            lock (_state.DiscoveryGate)
            {
                _state.CoreDiscoveryETag = value;
                _state.GroupedDiscoveryETag = value;
            }
        }
    }

    /// <summary>Gets or sets the ETag used for the core <c>/api</c> discovery response.</summary>
    public string CoreDiscoveryETag
    {
        get
        {
            lock (_state.DiscoveryGate)
            {
                return _state.CoreDiscoveryETag;
            }
        }
        set
        {
            lock (_state.DiscoveryGate)
            {
                _state.CoreDiscoveryETag = value;
            }
        }
    }

    /// <summary>Gets or sets the ETag used for grouped <c>/apis</c> discovery responses.</summary>
    public string GroupedDiscoveryETag
    {
        get
        {
            lock (_state.DiscoveryGate)
            {
                return _state.GroupedDiscoveryETag;
            }
        }
        set
        {
            lock (_state.DiscoveryGate)
            {
                _state.GroupedDiscoveryETag = value;
            }
        }
    }

    public string AuthenticatedUser { get; set; } = "system:admin";

    public bool FailConnection
    {
        get => _state.FailConnection;
        set => _state.FailConnection = value;
    }

    public bool ThrowOnConnection
    {
        get => _state.ThrowOnConnection;
        set => _state.ThrowOnConnection = value;
    }

    public int OpenApiV3IndexFailuresRemaining
    {
        get => Math.Max(0, Volatile.Read(ref _state.OpenApiV3IndexFailuresRemaining));
        set => Volatile.Write(ref _state.OpenApiV3IndexFailuresRemaining, Math.Max(0, value));
    }

    public HttpStatusCode OpenApiV3IndexStatusCode
    {
        get => (HttpStatusCode)Volatile.Read(ref _state.OpenApiV3IndexStatusCode);
        set => Volatile.Write(ref _state.OpenApiV3IndexStatusCode, (int)value);
    }

    public HttpStatusCode OpenApiV3DocumentStatusCode
    {
        get => (HttpStatusCode)Volatile.Read(ref _state.OpenApiV3DocumentStatusCode);
        set => Volatile.Write(ref _state.OpenApiV3DocumentStatusCode, (int)value);
    }

    /// <summary>Gets or sets the hash advertised for OpenAPI v3 group-version documents.</summary>
    public string OpenApiV3DocumentHash
    {
        get => _state.OpenApiV3DocumentHash;
        set => _state.OpenApiV3DocumentHash = value;
    }

    public IReadOnlyList<Uri?> RequestUris => _requestUris.ToArray();

    private int _authorizationRequestCount;
    private int _rulesReviewRequestCount;

    public int AuthorizationRequestCount => Volatile.Read(ref _authorizationRequestCount);

    public int RulesReviewRequestCount => Volatile.Read(ref _rulesReviewRequestCount);

    /// <summary>Gets or sets whether rules reviews report incomplete results.</summary>
    public bool SelfSubjectRulesReviewIncomplete { get; set; }

    /// <summary>Gets or sets an optional error status returned for rules reviews.</summary>
    public HttpStatusCode SelfSubjectRulesReviewStatusCode { get; set; }

    private readonly ConcurrentQueue<Uri?> _requestUris = new();

    public void Register<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var api = GroupApiVersionKind.From<T>();
        Register(api, IsNamespaced(typeof(T)));
    }

    internal void Register(GroupApiVersionKind api)
        => Register(api, IsNamespacedResource(api.PluralName));

    private void Register(GroupApiVersionKind api, bool namespaced)
    {
        var key = DefinitionKey(api.Group, api.ApiVersion, api.PluralName);
        if (_definitions.TryGetValue(key, out var existing)
            && !string.Equals(existing.Api.Kind, existing.Api.PluralName, StringComparison.Ordinal))
        {
            return;
        }

        var definition = CreateDefinition(api, namespaced);
        _definitions[key] = definition;
    }

    public void Add<T>(T resource) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        if (resource is GenericKubernetesObject metricsResource
            && TryGetMetricsResourceDefinition(metricsResource, out var metricsDefinition))
        {
            AddMetricsResource(metricsResource, metricsDefinition);
            return;
        }

        var api = GroupApiVersionKind.From<T>();
        var json = ParseObject(KubernetesJson.Serialize(resource));
        NormalizeSecret(json);
        EnsureMetadata(json, resource.Metadata);
        RegisterPolicyResources(json);

        if (resource is V1CustomResourceDefinition)
        {
            Register<T>();
            RegisterCustomResourceDefinition(json);
        }
        else if (string.IsNullOrWhiteSpace(api.PluralName)
            || resource is GenericKubernetesObject)
        {
            if (!TryFindDefinition(resource, out var definition))
            {
                throw new InvalidOperationException(
                    $"No registered Kubernetes API resource matches {resource.ApiVersion}/{resource.Kind}.");
            }

            api = definition.Api;
        }
        else
        {
            Register<T>();
        }

        json["apiVersion"] ??= string.IsNullOrEmpty(api.Group)
            ? api.ApiVersion
            : $"{api.Group}/{api.ApiVersion}";
        json["kind"] ??= api.Kind;
        _resources[ResourceKey(ResourcePath(api, resource.Metadata?.NamespaceProperty, resource.Metadata?.Name))] = json;
    }

    private bool TryFindDefinition(
        IKubernetesObject<V1ObjectMeta> resource,
        out ResourceDefinition definition)
    {
        var apiVersion = resource.ApiVersion ?? string.Empty;
        var separator = apiVersion.IndexOf('/', StringComparison.Ordinal);
        var group = separator < 0 ? string.Empty : apiVersion[..separator];
        var version = separator < 0 ? apiVersion : apiVersion[(separator + 1)..];
        foreach (var candidate in _definitions.Values)
        {
            if (string.Equals(candidate.Api.Group, group, StringComparison.Ordinal)
                && string.Equals(candidate.Api.ApiVersion, version, StringComparison.Ordinal)
                && string.Equals(candidate.Api.Kind, resource.Kind, StringComparison.Ordinal))
            {
                definition = candidate;
                return true;
            }
        }

        definition = default;
        return false;
    }

    public void Add(IKubernetesObject resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var add = GetType()
            .GetMethods()
            .Single(method => method.Name == nameof(Add) && method.IsGenericMethodDefinition);
        add.MakeGenericMethod(resource.GetType()).Invoke(this, new object[] { resource });
    }

    private void AddMetricsResource(
        GenericKubernetesObject resource,
        ResourceDefinition definition)
    {
        var key = DefinitionKey(definition.Api.Group, definition.Api.ApiVersion, definition.Api.PluralName);
        lock (_state.DiscoveryGate)
        {
            _definitions[key] = definition;
            InvalidateGroupedDiscoveryETag();
        }

        var json = ParseObject(KubernetesJson.Serialize(resource));
        EnsureMetadata(json, resource.Metadata);
        _resources[ResourceKey(ResourcePath(
            definition.Api,
            resource.Metadata.NamespaceProperty,
            resource.Metadata.Name))] = json;
    }

    private static bool TryGetMetricsResourceDefinition(
        GenericKubernetesObject resource,
        out ResourceDefinition definition)
    {
        if (resource.ApiVersion == "metrics.k8s.io/v1beta1"
            && resource.Kind is "NodeMetrics" or "PodMetrics")
        {
            var pluralName = resource.Kind == "NodeMetrics" ? "nodes" : "pods";
            definition = CreateDefinition(
                new GroupApiVersionKind("metrics.k8s.io", "v1beta1", resource.Kind, pluralName),
                resource.Kind == "PodMetrics");
            return true;
        }

        definition = default;
        return false;
    }

    public void AddYaml(string yaml, IDictionary<string, Type> typeMap)
    {
        foreach (var resource in KubeUI.Kubernetes.Serialization.KubernetesYaml.LoadAllFromString(yaml, typeMap))
        {
            var add = GetType()
                .GetMethods()
                .Single(method => method.Name == nameof(Add) && method.IsGenericMethodDefinition)
                .MakeGenericMethod(resource.GetType());
            add.Invoke(this, [resource]);
        }
    }

    public void SetPermission(string resource, string verb, bool allowed, string? @namespace = null, string? subresource = null)
    {
        _permissions[PermissionKey(resource, verb, @namespace, subresource)] = allowed;
    }

    /// <summary>
    /// Ends all outstanding watch streams. The transport is shared across client instances so that
    /// reconnect scenarios can dispose one Kubernetes client without disabling the fake backend.
    /// </summary>
    public void Shutdown()
    {
        _shutdownCancellation.Cancel();

        foreach (var watchers in _watchers.Values)
        {
            foreach (var watcher in watchers)
            {
                watcher.Writer.TryComplete();
            }
        }

        _watchers.Clear();
    }

    protected override void Dispose(bool disposing)
    {
        // The production Kubernetes client owns the handler it receives and disposes it when a
        // client disconnects. This fake is the shared backend owner; Shutdown is called by the
        // test harness after the final client has disconnected.
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _requestUris.Enqueue(request.RequestUri);
        cancellationToken.ThrowIfCancellationRequested();

        if (request.RequestUri is null)
        {
            return SetRequest(request, Error(HttpStatusCode.BadRequest, "Request URI is missing."));
        }

        var path = request.RequestUri.AbsolutePath.TrimEnd('/');
        if (ThrowOnConnection)
        {
            throw new HttpRequestException("simulated connection failure");
        }

        if (FailConnection)
        {
            return SetRequest(request, Error(HttpStatusCode.ServiceUnavailable, "simulated connection failure"));
        }

        if (path.Contains("selfsubjectrulesreviews", StringComparison.Ordinal))
        {
            return SetRequest(request, await HandleRulesReviewAsync(request, cancellationToken).ConfigureAwait(false));
        }

        if (path.Contains("selfsubjectaccessreviews", StringComparison.Ordinal))
        {
            return SetRequest(request, await HandleAuthorizationAsync(request, cancellationToken).ConfigureAwait(false));
        }

        if (request.Method == HttpMethod.Get && (path is "/api" or "/apis"))
        {
            if (Volatile.Read(ref _state.RequireAuthorizationForDiscovery) != 0
                && request.Headers.Authorization is null)
            {
                return SetRequest(request, Error(HttpStatusCode.Forbidden, "discovery requires authentication"));
            }

            var aggregated = AcceptsApiDiscovery(request);
            if (path == "/api")
            {
                return SetRequest(request, aggregated ? DiscoveryResponse(request, true) : Json(ApiVersions()));
            }

            return SetRequest(request, aggregated ? DiscoveryResponse(request, false) : Json(ApiGroupList()));
        }

        if (request.Method == HttpMethod.Get && path is "/version")
        {
            return SetRequest(request, Json(new
            {
                major = "1",
                minor = "36",
                gitVersion = "v1.36.1",
                gitCommit = "fake",
                gitTreeState = "clean",
                buildDate = "2026-06-01T00:00:00Z",
                goVersion = "go1.24.0",
                compiler = "gc",
                platform = "linux/amd64",
                apiVersion = "v1",
                kind = "Info",
            }));
        }

        if (request.Method == HttpMethod.Get && path is "/healthz" or "/livez" or "/readyz")
        {
            return SetRequest(request, Text("ok"));
        }

        if (request.Method == HttpMethod.Get && path == "/openapi/v3")
        {
            var statusCode = Volatile.Read(ref _state.OpenApiV3IndexStatusCode);
            if (statusCode != 0)
            {
                return SetRequest(request, Error((HttpStatusCode)statusCode, "simulated OpenAPI authorization failure"));
            }

            if (Interlocked.Decrement(ref _state.OpenApiV3IndexFailuresRemaining) >= 0)
            {
                return SetRequest(request, Error(HttpStatusCode.ServiceUnavailable, "simulated OpenAPI failure"));
            }

            return SetRequest(request, Json(OpenApiV3Index()));
        }

        if (request.Method == HttpMethod.Get && path == "/openapi/v3/api/v1")
        {
            var statusCode = Volatile.Read(ref _state.OpenApiV3DocumentStatusCode);
            if (statusCode != 0)
            {
                return SetRequest(request, Error((HttpStatusCode)statusCode, "simulated OpenAPI document authorization failure"));
            }

            return SetRequest(request, Json(OpenApiV3Document()));
        }

        if (request.Method == HttpMethod.Get && path == "/api/v1")
        {
            return SetRequest(request, Json(ResourceList(string.Empty, "v1")));
        }

        if (request.Method == HttpMethod.Get && TryGetGroupDiscoveryRoute(path, out var discoveryGroup, out var discoveryVersion))
        {
            if (discoveryVersion is null)
            {
                var groupResponse = ApiGroup(discoveryGroup);
                return SetRequest(
                    request,
                    groupResponse is null
                        ? Error(HttpStatusCode.NotFound, $"the server could not find the requested resource group {discoveryGroup}")
                        : Json(groupResponse));
            }

            var resourceList = ResourceList(discoveryGroup, discoveryVersion);
            if (resourceList["resources"] is not JsonArray { Count: > 0 })
            {
                return SetRequest(request, Error(HttpStatusCode.NotFound, $"the server could not find the requested resource {discoveryGroup}/{discoveryVersion}"));
            }

            return SetRequest(request, Json(resourceList));
        }

        var route = ParseRoute(path);
        if (route is null)
        {
            return SetRequest(request, Error(HttpStatusCode.NotFound, $"Unsupported Kubernetes path: {path}"));
        }

        return SetRequest(request, await HandleResourceAsync(request, route.Value, cancellationToken).ConfigureAwait(false));
    }

    private static HttpResponseMessage SetRequest(HttpRequestMessage request, HttpResponseMessage response)
    {
        response.RequestMessage = request;
        return response;
    }

    private HttpResponseMessage DiscoveryResponse(HttpRequestMessage request, bool core)
    {
        lock (_state.DiscoveryGate)
        {
            var payload = Discovery(core);
            var etag = core ? _state.CoreDiscoveryETag : _state.GroupedDiscoveryETag;
            if (request.Headers.TryGetValues("If-None-Match", out var values)
                && values.Contains(etag, StringComparer.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.NotModified)
                {
                    Headers = { ETag = new EntityTagHeaderValue(etag) },
                };
            }

            var response = Json(payload);
            response.Headers.ETag = new EntityTagHeaderValue(etag);
            return response;
        }
    }

    private async Task<HttpResponseMessage> HandleAuthorizationAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _authorizationRequestCount);
        var body = await request.Content!.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var root = JsonNode.Parse(body)?.AsObject();
        var attributes = root?["spec"]?["resourceAttributes"]?.AsObject();
        var nonResourceAttributes = root?["spec"]?["nonResourceAttributes"]?.AsObject();
        bool allowed;
        if (attributes is not null)
        {
            var resource = attributes["resource"]?.GetValue<string>() ?? string.Empty;
            var verb = attributes["verb"]?.GetValue<string>() ?? string.Empty;
            var group = attributes["group"]?.GetValue<string>() ?? string.Empty;
            var @namespace = attributes["namespace"]?.GetValue<string>();
            var subresource = attributes["subresource"]?.GetValue<string>();
            var name = attributes["name"]?.GetValue<string>();
            @namespace = string.IsNullOrEmpty(@namespace) ? null : @namespace;
            subresource = string.IsNullOrEmpty(subresource) ? null : subresource;
            name = string.IsNullOrEmpty(name) ? null : name;
            allowed = _permissions.TryGetValue(PermissionKey(resource, verb, @namespace, subresource), out var configured)
                ? configured
                : UseRoleBasedAuthorization
                    ? IsRoleBasedAccessAllowed(group, resource, verb, @namespace, subresource, name)
                    : true;
        }
        else if (nonResourceAttributes is not null)
        {
            var path = nonResourceAttributes["path"]?.GetValue<string>() ?? string.Empty;
            var verb = nonResourceAttributes["verb"]?.GetValue<string>() ?? string.Empty;
            allowed = !UseRoleBasedAuthorization || IsNonResourceAccessAllowed(path, verb);
        }
        else
        {
            return BadRequestStatus("SelfSubjectAccessReview requires resourceAttributes or nonResourceAttributes.");
        }

        return Json(new
        {
            apiVersion = "authorization.k8s.io/v1",
            kind = "SelfSubjectAccessReview",
            status = new
            {
                allowed,
                denied = false,
                reason = string.Empty,
                evaluationError = string.Empty,
            },
        }, HttpStatusCode.Created);
    }

    private bool IsNonResourceAccessAllowed(string path, string verb)
    {
        var subject = ParseServiceAccount(AuthenticatedUser);
        if (subject is null)
        {
            return false;
        }

        foreach (var binding in _resources.Values)
        {
            if (!string.Equals(binding["kind"]?.GetValue<string>(), "ClusterRoleBinding", StringComparison.Ordinal))
            {
                continue;
            }

            var clusterRoleBinding = binding.Deserialize<V1ClusterRoleBinding>();
            if (clusterRoleBinding is null || !IsSubjectMatch(clusterRoleBinding.Subjects, subject.Value))
            {
                continue;
            }

            var role = GetClusterRole(clusterRoleBinding.RoleRef?.Name);
            if (role?.Rules?.Any(rule =>
                rule.NonResourceURLs is { Count: > 0 }
                && rule.NonResourceURLs.Any(url => MatchesNonResourceUrl(url, path))
                && Matches(rule.Verbs, verb)) == true)
            {
                return true;
            }
        }

        return false;
    }

    private async Task<HttpResponseMessage> HandleRulesReviewAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _rulesReviewRequestCount);
        var body = await request.Content!.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var root = JsonNode.Parse(body)?.AsObject();
        var @namespace = root?["spec"]?["namespace"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(@namespace))
        {
            return Error(HttpStatusCode.BadRequest, "SelfSubjectRulesReview requires a namespace.");
        }

        var statusCode = SelfSubjectRulesReviewStatusCode;
        if (statusCode != 0)
        {
            return Error(statusCode, "simulated rules review failure.");
        }

        if (SelfSubjectRulesReviewIncomplete)
        {
            return RulesReviewResponse([], incomplete: true);
        }

        if (HasDeniedPermissionForNamespace(@namespace))
        {
            return RulesReviewResponse([], incomplete: true);
        }

        if (!UseRoleBasedAuthorization)
        {
            return RulesReviewResponse(
                [
                    new V1PolicyRule
                    {
                        ApiGroups = ["*"],
                        Resources = ["*"],
                        Verbs = ["*"],
                    },
                ],
                incomplete: false);
        }

        return RulesReviewResponse(GetSubjectRules(@namespace), incomplete: false);
    }

    private HttpResponseMessage RulesReviewResponse(
        IReadOnlyCollection<V1PolicyRule> rules,
        bool incomplete)
    {
        return Json(new
        {
            apiVersion = "authorization.k8s.io/v1",
            kind = "SelfSubjectRulesReview",
            status = new
            {
                resourceRules = rules
                    .Where(rule => rule.Resources is { Count: > 0 })
                    .Select(rule => new
                    {
                        verbs = rule.Verbs ?? [],
                        apiGroups = rule.ApiGroups ?? [],
                        resources = rule.Resources ?? [],
                        resourceNames = rule.ResourceNames ?? [],
                    }),
                nonResourceRules = rules
                    .Where(rule => rule.NonResourceURLs is { Count: > 0 })
                    .Select(rule => new
                    {
                        verbs = rule.Verbs ?? [],
                        nonResourceURLs = rule.NonResourceURLs ?? [],
                    }),
                incomplete,
                evaluationError = incomplete ? "simulated incomplete rules" : string.Empty,
            },
        }, HttpStatusCode.Created);
    }

    private List<V1PolicyRule> GetSubjectRules(string @namespace)
    {
        var subject = ParseServiceAccount(AuthenticatedUser);
        if (subject is null)
        {
            return [];
        }

        List<V1PolicyRule> rules = [];
        foreach (var binding in _resources.Values)
        {
            if (string.Equals(binding["kind"]?.GetValue<string>(), "ClusterRoleBinding", StringComparison.Ordinal))
            {
                var clusterRoleBinding = binding.Deserialize<V1ClusterRoleBinding>();
                if (clusterRoleBinding is not null && IsSubjectMatch(clusterRoleBinding.Subjects, subject.Value))
                {
                    var role = GetClusterRole(clusterRoleBinding.RoleRef?.Name);
                    if (role?.Rules is not null)
                    {
                        rules.AddRange(role.Rules);
                    }
                }
            }
            else if (string.Equals(binding["kind"]?.GetValue<string>(), "RoleBinding", StringComparison.Ordinal))
            {
                var roleBinding = binding.Deserialize<V1RoleBinding>();
                if (roleBinding is null
                    || !string.Equals(roleBinding.Metadata?.NamespaceProperty, @namespace, StringComparison.Ordinal)
                    || !IsSubjectMatch(roleBinding.Subjects, subject.Value))
                {
                    continue;
                }

                var roleRules = string.Equals(roleBinding.RoleRef?.Kind, "ClusterRole", StringComparison.Ordinal)
                    ? GetClusterRole(roleBinding.RoleRef.Name)?.Rules
                    : GetRole(roleBinding.RoleRef?.Name, roleBinding.Metadata?.NamespaceProperty)?.Rules;
                if (roleRules is not null)
                {
                    rules.AddRange(roleRules);
                }
            }
        }

        return rules;
    }

    private bool HasDeniedPermissionForNamespace(string @namespace)
    {
        foreach (var permission in _permissions)
        {
            var parts = permission.Key.Split('|');
            if (parts.Length != 4 || permission.Value)
            {
                continue;
            }

            var permissionNamespace = string.IsNullOrEmpty(parts[2]) ? null : parts[2];
            if (permissionNamespace is null || string.Equals(permissionNamespace, @namespace, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private async Task<HttpResponseMessage> HandleResourceAsync(HttpRequestMessage request, Route route, CancellationToken cancellationToken)
    {
        if (!IsAllowed(request, route))
        {
            return Error(HttpStatusCode.Forbidden, $"forbidden: {request.Method} {route.CollectionPath}");
        }

        if (!_definitions.TryGetValue(DefinitionKey(route.Group, route.ApiVersion, route.PluralName), out var definition))
        {
            return Error(HttpStatusCode.NotFound, $"the server could not find the requested resource {route.CollectionPath}");
        }

        var collectionKey = CollectionKey(route);
        var key = ResourceKey(route.ResourceName is null ? collectionKey : collectionKey + "/" + route.ResourceName);

        if (route.Subresource is not null)
        {
            return await HandleSubresourceAsync(request, route, definition, collectionKey, key, cancellationToken).ConfigureAwait(false);
        }

        if (request.Method == HttpMethod.Get)
        {
            if (IsWatchRequest(request))
            {
                return WatchResponse(collectionKey, request, cancellationToken);
            }

            if (route.ResourceName is null)
            {
                return ListResponse(request, route, definition);
            }

            return _resources.TryGetValue(key, out var resource)
                ? Json(resource)
                : NotFoundStatus(route);
        }

        if (request.Method == HttpMethod.Post)
        {
            return await CreateAsync(request, route, definition, collectionKey, cancellationToken).ConfigureAwait(false);
        }

        if (request.Method == HttpMethod.Put)
        {
            return await ReplaceAsync(request, route, definition, collectionKey, key, cancellationToken).ConfigureAwait(false);
        }

        if (request.Method == HttpMethod.Patch)
        {
            return await PatchAsync(request, route, definition, collectionKey, key, cancellationToken).ConfigureAwait(false);
        }

        if (request.Method == HttpMethod.Delete)
        {
            return await DeleteAsync(request, route, definition, collectionKey, key, cancellationToken).ConfigureAwait(false);
        }

        return Error(HttpStatusCode.MethodNotAllowed, $"Unsupported method: {request.Method}");
    }

    private async Task<HttpResponseMessage> CreateAsync(
        HttpRequestMessage request,
        Route route,
        ResourceDefinition definition,
        string collectionKey,
        CancellationToken cancellationToken)
    {
        var body = await ReadBodyAsync(request, cancellationToken).ConfigureAwait(false);
        if (!TryParseObject(body, out var resource))
        {
            return BadRequestStatus("the body of the request was in an unknown format");
        }

#pragma warning disable CA2000
        var validation = ValidateAndDefault(resource, route, definition, isCreate: true);
        if (validation is not null)
        {
            return validation;
        }
#pragma warning restore CA2000

        var name = resource["metadata"]!["name"]!.GetValue<string>();
        var resourceKey = ResourceKey(collectionKey + "/" + name);
        if (_resources.ContainsKey(resourceKey))
        {
            return AlreadyExistsStatus(route, name);
        }

        var dryRun = IsDryRun(request);
        NormalizeSecret(resource);
        if (!dryRun)
        {
            EnsureMetadata(resource, metadata: null, isCreate: true);
            if (resource["spec"] is JsonObject)
            {
                resource["metadata"]!["generation"] = 1;
            }

            _resources[resourceKey] = resource;
            if (route.PluralName == "customresourcedefinitions")
            {
                RegisterCustomResourceDefinition(resource);
            }

            PublishWatch(collectionKey, "ADDED", resource);
        }

        return Json(resource, HttpStatusCode.Created);
    }

    private async Task<HttpResponseMessage> ReplaceAsync(
        HttpRequestMessage request,
        Route route,
        ResourceDefinition definition,
        string collectionKey,
        string key,
        CancellationToken cancellationToken)
    {
        if (route.ResourceName is null)
        {
            return BadRequestStatus("a resource name is required for updates");
        }

        if (!_resources.TryGetValue(key, out var existing))
        {
            return NotFoundStatus(route);
        }

        var body = await ReadBodyAsync(request, cancellationToken).ConfigureAwait(false);
        if (!TryParseObject(body, out var resource))
        {
            return BadRequestStatus("the body of the request was in an unknown format");
        }

        var resourceVersion = resource["metadata"]?["resourceVersion"]?.GetValue<string>();
        if (string.IsNullOrEmpty(resourceVersion))
        {
            return BadRequestStatus("metadata.resourceVersion: Invalid value: must be specified for an update");
        }

        if (!string.Equals(resourceVersion, existing["metadata"]?["resourceVersion"]?.GetValue<string>(), StringComparison.Ordinal))
        {
            return ConflictStatus("the object has been modified; please apply your changes to the latest version and try again");
        }

#pragma warning disable CA2000
        var validation = ValidateAndDefault(resource, route, definition, isCreate: false);
        if (validation is not null)
        {
            return validation;
        }
#pragma warning restore CA2000

        PreserveServerFields(existing, resource, definition);
        BumpGeneration(existing, resource);
        NormalizeSecret(resource);
        if (IsDryRun(request))
        {
            resource["metadata"]!["resourceVersion"] = existing["metadata"]?["resourceVersion"]?.DeepClone();
            return Json(resource);
        }

        EnsureMetadata(resource, null);

        _resources[key] = resource;
        if (route.PluralName == "customresourcedefinitions")
        {
            RegisterCustomResourceDefinition(resource);
        }

        PublishWatch(collectionKey, "MODIFIED", resource);
        CompleteDeletionIfFinalizersCleared(collectionKey, key, resource);
        return Json(resource);
    }

    private async Task<HttpResponseMessage> PatchAsync(
        HttpRequestMessage request,
        Route route,
        ResourceDefinition definition,
        string collectionKey,
        string key,
        CancellationToken cancellationToken)
    {
        if (route.ResourceName is null)
        {
            return BadRequestStatus("a resource name is required for updates");
        }

        if (!_resources.TryGetValue(key, out var existing))
        {
            return NotFoundStatus(route);
        }

        var body = await ReadBodyAsync(request, cancellationToken).ConfigureAwait(false);
        var patchType = request.Content?.Headers.ContentType?.MediaType;
        var resource = existing.DeepClone().AsObject();
        if (!TryApplyPatch(resource, body, patchType, out var patchError))
        {
            return BadRequestStatus(patchError);
        }

        var patchResourceVersion = resource["metadata"]?["resourceVersion"]?.GetValue<string>();
        if (!string.IsNullOrEmpty(patchResourceVersion)
            && !string.Equals(patchResourceVersion, existing["metadata"]?["resourceVersion"]?.GetValue<string>(), StringComparison.Ordinal))
        {
            return ConflictStatus("the object has been modified; please apply your changes to the latest version and try again");
        }

#pragma warning disable CA2000
        var validation = ValidateAndDefault(resource, route, definition, isCreate: false);
        if (validation is not null)
        {
            return validation;
        }
#pragma warning restore CA2000

        PreserveServerFields(existing, resource, definition);
        BumpGeneration(existing, resource);
        NormalizeSecret(resource);
        if (IsDryRun(request))
        {
            resource["metadata"]!["resourceVersion"] = existing["metadata"]?["resourceVersion"]?.DeepClone();
            return Json(resource);
        }

        EnsureMetadata(resource, null);

        _resources[key] = resource;
        if (route.PluralName == "customresourcedefinitions")
        {
            RegisterCustomResourceDefinition(resource);
        }

        PublishWatch(collectionKey, "MODIFIED", resource);
        CompleteDeletionIfFinalizersCleared(collectionKey, key, resource);
        return Json(resource);
    }

    private async Task<HttpResponseMessage> DeleteAsync(
        HttpRequestMessage request,
        Route route,
        ResourceDefinition definition,
        string collectionKey,
        string key,
        CancellationToken cancellationToken)
    {
        var dryRun = IsDryRun(request);
        var options = await ReadDeleteOptionsAsync(request, cancellationToken).ConfigureAwait(false);

        if (route.ResourceName is null)
        {
            return DeleteCollection(options, dryRun, collectionKey);
        }

        if (!_resources.TryGetValue(key, out var existing))
        {
            return NotFoundStatus(route);
        }

        var precondition = CheckDeletePreconditions(options, existing, route);
        if (precondition is not null)
        {
            return precondition;
        }

        if (dryRun)
        {
            return DeletedResourceResponse(route, definition, existing);
        }

        if (HasFinalizers(existing) && existing["metadata"]?["deletionTimestamp"] is null)
        {
            var metadata = existing["metadata"]!.AsObject();
            metadata["deletionTimestamp"] = DateTime.UtcNow;
            metadata["resourceVersion"] = CurrentResourceVersion();
            _resources[key] = existing;
            PublishWatch(collectionKey, "MODIFIED", existing);
            return DeletedResourceResponse(route, definition, existing);
        }

        _resources.TryRemove(key, out _);
        if (route.PluralName == "customresourcedefinitions")
        {
            UnregisterCustomResourceDefinition(existing);
        }

        PublishWatch(collectionKey, "DELETED", existing);
        return DeletedResourceResponse(route, definition, existing);
    }

    private static HttpResponseMessage DeletedResourceResponse(Route route, ResourceDefinition definition, JsonObject resource)
        => HasFinalizers(resource) || definition.HasStatusSubresource || route.PluralName == "pods"
            ? Json(resource)
            : DeleteStatus(route, resource);

    private static HttpResponseMessage DeleteStatus(Route route, JsonObject resource)
    {
        var metadata = resource["metadata"];
        var details = new JsonObject
        {
            ["name"] = metadata?["name"]?.DeepClone(),
            ["kind"] = route.PluralName,
            ["uid"] = metadata?["uid"]?.DeepClone(),
        };
        if (!string.IsNullOrEmpty(route.Group))
        {
            details["group"] = route.Group;
        }

        return Json(new JsonObject
        {
            ["apiVersion"] = "v1",
            ["kind"] = "Status",
            ["status"] = "Success",
            ["details"] = details,
            ["code"] = (int)HttpStatusCode.OK,
        });
    }

    private HttpResponseMessage DeleteCollection(JsonObject? options, bool dryRun, string collectionKey)
    {
        foreach (var pair in _resources.Where(pair => IsResourceInCollection(pair.Key, collectionKey)).ToArray())
        {
            var resource = pair.Value;
            var precondition = CheckDeletePreconditions(options, resource, route: null);
            if (precondition is not null)
            {
                return precondition;
            }

            if (dryRun)
            {
                continue;
            }

            if (HasFinalizers(resource) && resource["metadata"]?["deletionTimestamp"] is null)
            {
                resource["metadata"]!.AsObject()["deletionTimestamp"] = DateTime.UtcNow;
                resource["metadata"]!.AsObject()["resourceVersion"] = CurrentResourceVersion();
                _resources[pair.Key] = resource;
                PublishWatch(collectionKey, "MODIFIED", resource);
                continue;
            }

            _resources.TryRemove(pair.Key, out _);
            PublishWatch(collectionKey, "DELETED", resource);
        }

        return Json(new JsonObject
        {
            ["apiVersion"] = "v1",
            ["kind"] = "Status",
            ["status"] = "Success",
            ["details"] = new JsonObject { ["kind"] = collectionKey.Split('/').Last() },
            ["code"] = (int)HttpStatusCode.OK,
        });
    }

    private HttpResponseMessage? CheckDeletePreconditions(JsonObject? options, JsonObject resource, Route? route)
    {
        var preconditions = options?["preconditions"] as JsonObject;
        if (preconditions is null)
        {
            return null;
        }

        var expectedUid = preconditions["uid"]?.GetValue<string>();
        if (!string.IsNullOrEmpty(expectedUid)
            && !string.Equals(expectedUid, resource["metadata"]?["uid"]?.GetValue<string>(), StringComparison.Ordinal))
        {
            return ConflictStatus("the UID in the precondition does not match the object's UID");
        }

        var expectedVersion = preconditions["resourceVersion"]?.GetValue<string>();
        if (!string.IsNullOrEmpty(expectedVersion)
            && !string.Equals(expectedVersion, resource["metadata"]?["resourceVersion"]?.GetValue<string>(), StringComparison.Ordinal))
        {
            return ConflictStatus("the resourceVersion in the precondition does not match the object's resourceVersion");
        }

        _ = route;
        return null;
    }

    private async Task<HttpResponseMessage> HandleSubresourceAsync(
        HttpRequestMessage request,
        Route route,
        ResourceDefinition definition,
        string collectionKey,
        string key,
        CancellationToken cancellationToken)
    {
        if (!_resources.TryGetValue(key, out var existing))
        {
            return NotFoundStatus(route);
        }

        if (route.Subresource == "log")
        {
            return request.Method == HttpMethod.Get
                ? Text(string.Empty)
                : Error(HttpStatusCode.MethodNotAllowed, "Only GET is supported for the log subresource.");
        }

        if (route.Subresource == "ephemeralcontainers")
        {
            if (request.Method != HttpMethod.Put && request.Method != HttpMethod.Patch)
            {
                return request.Method == HttpMethod.Get ? Json(existing) : Error(HttpStatusCode.MethodNotAllowed, "Unsupported method for the ephemeralcontainers subresource.");
            }

            var body = await ReadBodyAsync(request, cancellationToken).ConfigureAwait(false);
            if (!TryParseObject(body, out var parsedPatch))
            {
                return BadRequestStatus("the body of the request was in an unknown format");
            }

            var ephemeralContainers = parsedPatch["spec"]?["ephemeralContainers"]?.DeepClone();
            if (ephemeralContainers is null)
            {
                return BadRequestStatus("spec.ephemeralContainers is required");
            }

            var updated = existing.DeepClone().AsObject();
            updated["spec"] ??= new JsonObject();
            updated["spec"]!["ephemeralContainers"] = ephemeralContainers;
            updated["metadata"]!["resourceVersion"] = CurrentResourceVersion();
            _resources[key] = updated;
            PublishWatch(collectionKey, "MODIFIED", updated);
            return Json(updated);
        }

        if (route.Subresource == "status")
        {
            if (request.Method == HttpMethod.Get)
            {
                return Json(existing);
            }

            if (request.Method != HttpMethod.Put && request.Method != HttpMethod.Patch)
            {
                return Error(HttpStatusCode.MethodNotAllowed, "Only GET, PUT and PATCH are supported for the status subresource.");
            }

            var body = await ReadBodyAsync(request, cancellationToken).ConfigureAwait(false);
            var updated = existing.DeepClone().AsObject();
            if (!TryApplyPatch(updated, body, request.Content?.Headers.ContentType?.MediaType, out var statusError))
            {
                return BadRequestStatus(statusError);
            }

            updated["status"] = TryParseObject(body, out var statusPatch)
                ? statusPatch["status"]?.DeepClone() ?? new JsonObject()
                : new JsonObject();
            updated["metadata"]!["resourceVersion"] = CurrentResourceVersion();
            _resources[key] = updated;
            PublishWatch(collectionKey, "MODIFIED", updated);
            return Json(updated);
        }

        if (route.Subresource == "scale" && definition.SupportsScale)
        {
            if (request.Method == HttpMethod.Get)
            {
                return Json(ScaleObject(existing));
            }

            if (request.Method != HttpMethod.Put && request.Method != HttpMethod.Patch)
            {
                return Error(HttpStatusCode.MethodNotAllowed, "Only GET, PUT and PATCH are supported for the scale subresource.");
            }

            var body = await ReadBodyAsync(request, cancellationToken).ConfigureAwait(false);
            if (!TryParseObject(body, out var scaleRequest))
            {
                return BadRequestStatus("the body of the request was in an unknown format");
            }

            var replicas = scaleRequest["spec"]?["replicas"];
            if (replicas is null)
            {
                return BadRequestStatus("spec.replicas is required for the scale subresource");
            }

            var updated = existing.DeepClone().AsObject();
            updated["spec"] ??= new JsonObject();
            updated["spec"]!["replicas"] = replicas.DeepClone();
            updated["metadata"]!["resourceVersion"] = CurrentResourceVersion();
            _resources[key] = updated;
            PublishWatch(collectionKey, "MODIFIED", updated);
            return Json(ScaleObject(updated));
        }

        return Error(HttpStatusCode.NotImplemented, $"The fake Kubernetes API does not implement the {route.Subresource} subresource.");
    }

    private static JsonObject ScaleObject(JsonObject resource)
    {
        var replicas = resource["spec"]?["replicas"]?.GetValue<int?>() ?? 0;
        var statusReplicas = resource["status"]?["replicas"]?.GetValue<int?>() ?? replicas;
        var selector = resource["status"]?["selector"]?.DeepClone()
            ?? new JsonObject
            {
                ["matchLabels"] = resource["spec"]?["selector"]?["matchLabels"]?.DeepClone() ?? new JsonObject(),
            };

        return new JsonObject
        {
            ["apiVersion"] = "autoscaling/v1",
            ["kind"] = "Scale",
            ["metadata"] = new JsonObject
            {
                ["name"] = resource["metadata"]?["name"]?.DeepClone(),
                ["namespace"] = resource["metadata"]?["namespace"]?.DeepClone(),
                ["uid"] = resource["metadata"]?["uid"]?.DeepClone(),
                ["resourceVersion"] = resource["metadata"]?["resourceVersion"]?.DeepClone(),
            },
            ["spec"] = new JsonObject { ["replicas"] = replicas },
            ["status"] = new JsonObject
            {
                ["replicas"] = statusReplicas,
                ["selector"] = selector,
            },
        };
    }

    private HttpResponseMessage ListResponse(HttpRequestMessage request, Route route, ResourceDefinition definition)
    {
        var collectionKey = CollectionKey(route);
        IEnumerable<KeyValuePair<string, JsonObject>> matching = _resources
            .Where(pair => IsResourceInCollection(pair.Key, collectionKey))
            .OrderBy(pair => pair.Key, StringComparer.Ordinal);

        var labelSelector = GetQueryValue(request, "labelSelector");
        if (!string.IsNullOrWhiteSpace(labelSelector))
        {
            matching = matching.Where(pair => MatchesLabelSelector(pair.Value, labelSelector));
        }

        var fieldSelector = GetQueryValue(request, "fieldSelector");
        if (!string.IsNullOrWhiteSpace(fieldSelector))
        {
            matching = matching.Where(pair => MatchesFieldSelector(pair.Value, fieldSelector));
        }

        var items = matching.Select(pair => pair.Value).ToList();
        var total = items.Count;
        var offset = DecodeContinueToken(GetQueryValue(request, "continue"));
        if (offset < 0 || offset > total)
        {
            return BadRequestStatus("continue: Invalid value: the provided continue token is not valid");
        }

        var limit = int.TryParse(GetQueryValue(request, "limit"), out var parsedLimit) && parsedLimit > 0
            ? parsedLimit
            : (int?)null;
        var page = limit is null ? items.Skip(offset).ToList() : items.Skip(offset).Take(limit.Value).ToList();
        var metadata = new JsonObject { ["resourceVersion"] = PeekResourceVersion() };
        if (limit is not null && offset + page.Count < total)
        {
            metadata["continue"] = EncodeContinueToken(offset + page.Count);
            metadata["remainingItemCount"] = total - offset - page.Count;
        }

        var apiVersion = string.IsNullOrEmpty(definition.Api.Group)
            ? definition.Api.ApiVersion
            : $"{definition.Api.Group}/{definition.Api.ApiVersion}";

        return Json(new JsonObject
        {
            ["apiVersion"] = apiVersion,
            ["kind"] = definition.ListKind,
            ["metadata"] = metadata,
            ["items"] = new JsonArray(page.Select(item => (JsonNode?)item.DeepClone()).ToArray()),
        });
    }

    private HttpResponseMessage? ValidateAndDefault(JsonObject resource, Route route, ResourceDefinition definition, bool isCreate)
    {
        var metadata = resource["metadata"] as JsonObject;
        if (metadata is null)
        {
            metadata = new JsonObject();
            resource["metadata"] = metadata;
        }

        var name = metadata["name"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(name))
        {
            var generateName = metadata["generateName"]?.GetValue<string>();
            if (!isCreate || string.IsNullOrWhiteSpace(generateName))
            {
                return InvalidStatus("metadata.name", "name or generateName is required", "FieldValueRequired");
            }

            name = generateName + Guid.NewGuid().ToString("N")[..5];
            metadata["name"] = name;
        }

        if (!IsDnsSubdomain(name))
        {
            return InvalidStatus(
                "metadata.name",
                $"a lowercase RFC 1123 subdomain must consist of lower case alphanumeric characters, '-' or '.', and must start and end with an alphanumeric character",
                "FieldValueInvalid");
        }

        var bodyNamespace = metadata["namespace"]?.GetValue<string>();
        if (definition.Namespaced)
        {
            if (!string.IsNullOrEmpty(route.Namespace))
            {
                if (!string.IsNullOrEmpty(bodyNamespace)
                    && !string.Equals(bodyNamespace, route.Namespace, StringComparison.Ordinal))
                {
                    return BadRequestStatus("the namespace of the provided object does not match the namespace sent on the request");
                }

                metadata["namespace"] = route.Namespace;
            }
            else if (string.IsNullOrEmpty(bodyNamespace))
            {
                metadata["namespace"] = "default";
            }
            else
            {
                metadata["namespace"] = bodyNamespace;
            }

            var resolvedNamespace = metadata["namespace"]!.GetValue<string>();
            if (isCreate && !NamespaceExists(resolvedNamespace))
            {
                return Error(HttpStatusCode.NotFound, $"namespaces \"{resolvedNamespace}\" not found");
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(route.Namespace))
            {
                return BadRequestStatus($"the server could not find the requested resource: namespaces/{route.Namespace}/{route.PluralName}");
            }

            metadata.Remove("namespace");
        }

        var expectedApiVersion = string.IsNullOrEmpty(route.Group)
            ? route.ApiVersion
            : $"{route.Group}/{route.ApiVersion}";
        var apiVersion = resource["apiVersion"]?.GetValue<string>();
        if (!string.IsNullOrEmpty(apiVersion) && !string.Equals(apiVersion, expectedApiVersion, StringComparison.Ordinal))
        {
            return BadRequestStatus($"the API version in the data ({apiVersion}) does not match the expected API version ({expectedApiVersion})");
        }

        var kind = resource["kind"]?.GetValue<string>();
        if (!string.IsNullOrEmpty(kind) && !string.Equals(kind, definition.Api.Kind, StringComparison.Ordinal))
        {
            return BadRequestStatus($"the kind in the data ({kind}) does not match the expected kind ({definition.Api.Kind})");
        }

        ApplyDefaults(resource, definition);
        return ValidateStructure(resource, definition);
    }

    private static HttpResponseMessage? ValidateStructure(JsonObject resource, ResourceDefinition definition)
    {
        if (string.IsNullOrEmpty(definition.Api.Group) && definition.Api.PluralName == "pods")
        {
            if (resource["spec"]?["containers"] is not JsonArray { Count: > 0 } containers)
            {
                return InvalidStatus("spec.containers", "Required value: must specify at least one container", "FieldValueRequired");
            }

            for (var index = 0; index < containers.Count; index++)
            {
                var container = containers[index] as JsonObject;
                if (container is null || string.IsNullOrWhiteSpace(container["name"]?.GetValue<string>()))
                {
                    return InvalidStatus($"spec.containers[{index}].name", "Required value", "FieldValueRequired");
                }

                if (string.IsNullOrWhiteSpace(container["image"]?.GetValue<string>()))
                {
                    return InvalidStatus($"spec.containers[{index}].image", "Required value", "FieldValueRequired");
                }

                var pullPolicy = container["imagePullPolicy"]?.GetValue<string>();
                if (pullPolicy is not null
                    && pullPolicy is not ("Always" or "IfNotPresent" or "Never"))
                {
                    return InvalidStatus(
                        $"spec.containers[{index}].imagePullPolicy",
                        $"Unsupported value: {pullPolicy}. Supported values are Always, IfNotPresent, and Never.",
                        "FieldValueNotSupported");
                }
            }
        }

        return null;
    }

    private void ApplyDefaults(JsonObject resource, ResourceDefinition definition)
    {
        if (definition.Api.Group.Length == 0)
        {
            switch (definition.Api.PluralName)
            {
                case "pods":
                    ApplyPodDefaults(resource);
                    break;
                case "services":
                    ApplyServiceDefaults(resource);
                    break;
                case "secrets":
                    resource["type"] ??= "Opaque";
                    break;
                case "namespaces":
                    resource["status"] ??= new JsonObject();
                    resource["status"]!["phase"] ??= "Active";
                    break;
            }
        }

        if (definition.Api.Group == "apps")
        {
            switch (definition.Api.PluralName)
            {
                case "deployments":
                    ApplyDeploymentDefaults(resource);
                    break;
                case "replicasets":
                case "statefulsets":
                    resource["spec"] ??= new JsonObject();
                    resource["spec"]!["replicas"] ??= 1;
                    break;
            }
        }
    }

    private static void ApplyPodDefaults(JsonObject resource)
    {
        resource["spec"] ??= new JsonObject();
        var spec = resource["spec"]!.AsObject();
        spec["restartPolicy"] ??= "Always";
        spec["dnsPolicy"] ??= "ClusterFirst";
        spec["terminationGracePeriodSeconds"] ??= 30;
        spec["schedulerName"] ??= "default-scheduler";
        spec["serviceAccountName"] ??= "default";
        spec["enableServiceLinks"] ??= true;
        spec["priority"] ??= 0;
        spec["securityContext"] ??= new JsonObject();

        if (spec["containers"] is not JsonArray containers)
        {
            return;
        }

        foreach (var node in containers.OfType<JsonObject>())
        {
            var image = node["image"]?.GetValue<string>() ?? string.Empty;
            node["imagePullPolicy"] ??= image.EndsWith(":latest", StringComparison.Ordinal) || !image.Contains(':', StringComparison.Ordinal)
                ? "Always"
                : "IfNotPresent";
            node["terminationMessagePath"] ??= "/dev/termination-log";
            node["terminationMessagePolicy"] ??= "File";
            node["resources"] ??= new JsonObject();
        }
    }

    private void ApplyServiceDefaults(JsonObject resource)
    {
        resource["spec"] ??= new JsonObject();
        var spec = resource["spec"]!.AsObject();
        spec["type"] ??= "ClusterIP";
        spec["sessionAffinity"] ??= "None";
        spec["internalTrafficPolicy"] ??= "Cluster";
        spec["ipFamilies"] ??= new JsonArray("IPv4");
        spec["ipFamilyPolicy"] ??= "SingleStack";
        var type = spec["type"]!.GetValue<string>();
        if (type is "ClusterIP" or "NodePort" or "LoadBalancer")
        {
            spec["clusterIP"] ??= "10.96.0." + (Interlocked.Increment(ref _state.ServiceIpCounter) % 250 + 2).ToString(System.Globalization.CultureInfo.InvariantCulture);
            spec["clusterIPs"] ??= new JsonArray(spec["clusterIP"]!.GetValue<string>());
        }
    }

    private static void ApplyDeploymentDefaults(JsonObject resource)
    {
        resource["spec"] ??= new JsonObject();
        var spec = resource["spec"]!.AsObject();
        spec["replicas"] ??= 1;
        spec["revisionHistoryLimit"] ??= 10;
        spec["progressDeadlineSeconds"] ??= 600;
        spec["strategy"] ??= new JsonObject
        {
            ["type"] = "RollingUpdate",
            ["rollingUpdate"] = new JsonObject { ["maxSurge"] = "25%", ["maxUnavailable"] = "25%" },
        };
    }

    private static void PreserveServerFields(JsonObject existing, JsonObject resource, ResourceDefinition definition)
    {
        var existingMetadata = existing["metadata"] as JsonObject;
        var metadata = resource["metadata"] as JsonObject ?? new JsonObject();
        resource["metadata"] = metadata;
        if (existingMetadata is not null)
        {
            metadata["uid"] = existingMetadata["uid"]?.DeepClone();
            metadata["creationTimestamp"] = existingMetadata["creationTimestamp"]?.DeepClone();
            metadata["deletionTimestamp"] = existingMetadata["deletionTimestamp"]?.DeepClone();
            metadata["generation"] = existingMetadata["generation"]?.DeepClone();
        }

        if (definition.HasStatusSubresource)
        {
            if (existing["status"] is null)
            {
                resource.Remove("status");
            }
            else
            {
                resource["status"] = existing["status"]!.DeepClone();
            }
        }
    }

    private static void BumpGeneration(JsonObject existing, JsonObject resource)
    {
        var metadata = resource["metadata"] as JsonObject;
        if (metadata is null)
        {
            return;
        }

        var generation = existing["metadata"]?["generation"]?.GetValue<int?>() ?? 0;
        if (JsonNode.DeepEquals(existing["spec"], resource["spec"]) || resource["spec"] is null)
        {
            if (generation > 0)
            {
                metadata["generation"] = generation;
            }
        }
        else
        {
            metadata["generation"] = generation + 1;
        }
    }

    private void CompleteDeletionIfFinalizersCleared(string collectionKey, string key, JsonObject resource)
    {
        if (resource["metadata"]?["deletionTimestamp"] is null || HasFinalizers(resource))
        {
            return;
        }

        _resources.TryRemove(key, out _);
        PublishWatch(collectionKey, "DELETED", resource);
    }

    private static bool HasFinalizers(JsonObject resource)
        => resource["metadata"]?["finalizers"] is JsonArray { Count: > 0 };

    private bool NamespaceExists(string @namespace)
    {
        if (@namespace == "default")
        {
            return true;
        }

        return _resources.ContainsKey(ResourceKey($"/api/v1/namespaces/{@namespace}"));
    }

    private static bool TryApplyPatch(JsonObject target, string body, string? mediaType, out string error)
    {
        error = string.Empty;
        if (string.Equals(mediaType, "application/json-patch+json", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var operations = JsonNode.Parse(body) as JsonArray;
                if (operations is null)
                {
                    error = "a JSON patch must be an array of operations";
                    return false;
                }

                ApplyJsonPatch(target, operations);
                return true;
            }
            catch (Exception exception) when (exception is JsonException or InvalidOperationException or ArgumentException)
            {
                error = exception.Message;
                return false;
            }
        }

        if (!TryParseObject(body, out var patch))
        {
            error = "the body of the request was in an unknown format";
            return false;
        }

        var strategic = string.Equals(mediaType, "application/strategic-merge-patch+json", StringComparison.OrdinalIgnoreCase);
        Merge(target, patch, strategic);
        return true;
    }

    private static void ApplyJsonPatch(JsonObject target, JsonArray operations)
    {
        foreach (var operation in operations.OfType<JsonObject>())
        {
            var op = operation["op"]?.GetValue<string>()
                ?? throw new InvalidOperationException("JSON patch operation is missing 'op'.");
            var path = operation["path"]?.GetValue<string>()
                ?? throw new InvalidOperationException("JSON patch operation is missing 'path'.");

            switch (op)
            {
                case "add":
                    SetJsonPointer(target, path, operation["value"]?.DeepClone(), add: true);
                    break;
                case "replace":
                    SetJsonPointer(target, path, operation["value"]?.DeepClone(), add: false);
                    break;
                case "remove":
                    RemoveJsonPointer(target, path);
                    break;
                case "copy":
                    {
                        var from = operation["from"]?.GetValue<string>()
                            ?? throw new InvalidOperationException("JSON patch copy is missing 'from'.");
                        SetJsonPointer(target, path, GetJsonPointer(target, from)?.DeepClone(), add: true);
                        break;
                    }
                case "move":
                    {
                        var from = operation["from"]?.GetValue<string>()
                            ?? throw new InvalidOperationException("JSON patch move is missing 'from'.");
                        var value = GetJsonPointer(target, from)?.DeepClone();
                        RemoveJsonPointer(target, from);
                        SetJsonPointer(target, path, value, add: true);
                        break;
                    }
                case "test":
                    {
                        var expected = operation["value"];
                        var actual = GetJsonPointer(target, path);
                        if (!JsonNode.DeepEquals(actual, expected))
                        {
                            throw new InvalidOperationException($"JSON patch test failed at {path}.");
                        }

                        break;
                    }
                default:
                    throw new InvalidOperationException($"Unsupported JSON patch operation '{op}'.");
            }
        }
    }

    private static JsonNode? GetJsonPointer(JsonObject root, string pointer)
    {
        JsonNode? current = root;
        foreach (var token in ParseJsonPointer(pointer))
        {
            current = current switch
            {
                JsonObject obj when obj.TryGetPropertyValue(token, out var value) => value,
                JsonArray array when int.TryParse(token, out var index) && index >= 0 && index < array.Count => array[index],
                _ => null,
            };
        }

        return current;
    }

    private static void SetJsonPointer(JsonObject root, string pointer, JsonNode? value, bool add)
    {
        var tokens = ParseJsonPointer(pointer);
        if (tokens.Length == 0)
        {
            if (value is not JsonObject replacement)
            {
                throw new InvalidOperationException("the root of a JSON patch must be an object");
            }

            foreach (var property in root.Select(property => property.Key).ToArray())
            {
                root.Remove(property);
            }

            foreach (var property in replacement)
            {
                root[property.Key] = property.Value?.DeepClone();
            }

            return;
        }

        var parent = ResolveJsonPointerParent(root, tokens);
        var last = tokens[^1];
        switch (parent)
        {
            case JsonObject obj:
                if (!add && !obj.ContainsKey(last))
                {
                    throw new InvalidOperationException($"JSON patch path {pointer} does not exist.");
                }

                obj[last] = value;
                break;
            case JsonArray array when last == "-":
                array.Add(value);
                break;
            case JsonArray array when int.TryParse(last, out var index):
                if (add)
                {
                    if (index < 0 || index > array.Count)
                    {
                        throw new InvalidOperationException($"JSON patch index {index} is out of range.");
                    }

                    array.Insert(index, value);
                }
                else
                {
                    if (index < 0 || index >= array.Count)
                    {
                        throw new InvalidOperationException($"JSON patch index {index} is out of range.");
                    }

                    array[index] = value;
                }

                break;
            default:
                throw new InvalidOperationException($"JSON patch path {pointer} does not resolve to a container.");
        }
    }

    private static void RemoveJsonPointer(JsonObject root, string pointer)
    {
        var tokens = ParseJsonPointer(pointer);
        if (tokens.Length == 0)
        {
            throw new InvalidOperationException("cannot remove the JSON document root");
        }

        var parent = ResolveJsonPointerParent(root, tokens);
        var last = tokens[^1];
        switch (parent)
        {
            case JsonObject obj when obj.Remove(last):
                break;
            case JsonArray array when int.TryParse(last, out var index) && index >= 0 && index < array.Count:
                array.RemoveAt(index);
                break;
            default:
                throw new InvalidOperationException($"JSON patch path {pointer} does not exist.");
        }
    }

    private static JsonNode ResolveJsonPointerParent(JsonObject root, string[] tokens)
    {
        JsonNode current = root;
        for (var index = 0; index < tokens.Length - 1; index++)
        {
            var token = tokens[index];
            current = current switch
            {
                JsonObject obj when obj.TryGetPropertyValue(token, out var value) && value is not null => value,
                JsonArray array when int.TryParse(token, out var arrayIndex) && arrayIndex >= 0 && arrayIndex < array.Count => array[arrayIndex]!,
                _ => throw new InvalidOperationException($"JSON patch path segment '{token}' does not exist."),
            };
        }

        return current;
    }

    private static string[] ParseJsonPointer(string pointer)
    {
        if (string.IsNullOrEmpty(pointer) || pointer == "/")
        {
            return [];
        }

        if (!pointer.StartsWith('/'))
        {
            throw new InvalidOperationException("a JSON pointer must start with '/'.");
        }

        return pointer[1..]
            .Split('/')
            .Select(token => token.Replace("~1", "/", StringComparison.Ordinal).Replace("~0", "~", StringComparison.Ordinal))
            .ToArray();
    }

    private static bool TryParseObject(string body, out JsonObject resource)
    {
        try
        {
            resource = JsonNode.Parse(body)?.AsObject()
                ?? throw new JsonException("the body of the request was in an unknown format");
            return true;
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException)
        {
            resource = new JsonObject();
            return false;
        }
    }

    private static async Task<string> ReadBodyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return request.Content is null
            ? string.Empty
            : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<JsonObject?> ReadDeleteOptionsAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content is null)
        {
            return null;
        }

        var body = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(body) || !TryParseObject(body, out var options) ? null : options;
    }

    private static bool IsWatchRequest(HttpRequestMessage request)
    {
        if (!string.Equals(request.Method.Method, "GET", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var query = request.RequestUri?.Query;
        if (string.IsNullOrEmpty(query))
        {
            return false;
        }

        foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = pair.IndexOf('=');
            var key = separator < 0 ? pair : pair[..separator];
            var value = separator < 0 ? string.Empty : pair[(separator + 1)..];
            if (string.Equals(Uri.UnescapeDataString(key), "watch", StringComparison.Ordinal))
            {
                return value is "1" or "true" or "True";
            }
        }

        return false;
    }

    private static string? GetQueryValue(HttpRequestMessage request, string name)
    {
        var query = request.RequestUri?.Query;
        if (string.IsNullOrEmpty(query))
        {
            return null;
        }

        foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = pair.IndexOf('=');
            var key = separator < 0 ? pair : pair[..separator];
            if (!string.Equals(Uri.UnescapeDataString(key), name, StringComparison.Ordinal))
            {
                continue;
            }

            return separator < 0
                ? string.Empty
                : Uri.UnescapeDataString(pair[(separator + 1)..].Replace("+", " ", StringComparison.Ordinal));
        }

        return null;
    }

    private static bool IsDryRun(HttpRequestMessage request)
    {
        var dryRun = GetQueryValue(request, "dryRun");
        return !string.IsNullOrWhiteSpace(dryRun)
            && !string.Equals(dryRun, "None", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDnsSubdomain(string value)
        => value.Length is > 0 and <= 253
            && value.Split('.').All(label =>
                label.Length is > 0 and <= 63
                && label[0] != '-'
                && label[^1] != '-'
                && label.All(character => char.IsAsciiLetterOrDigit(character) || character == '-'));

    private static string EncodeContinueToken(int offset)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(offset.ToString(System.Globalization.CultureInfo.InvariantCulture)));

    private static int DecodeContinueToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return 0;
        }

        try
        {
            var value = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            return int.TryParse(value, out var offset) && offset >= 0 ? offset : -1;
        }
        catch (FormatException)
        {
            return -1;
        }
    }

    private static bool MatchesLabelSelector(JsonObject resource, string selector)
    {
        return SplitSelector(selector).All(requirement =>
        {
            var (key, operation, values) = ParseSelectorRequirement(requirement);
            var labels = resource["metadata"]?["labels"] as JsonObject;
            JsonNode? node = null;
            var hasValue = labels?.TryGetPropertyValue(key, out node) == true;
            var value = hasValue ? node?.GetValue<string>() : null;
            return operation switch
            {
                SelectorOperator.Exists => hasValue,
                SelectorOperator.DoesNotExist => !hasValue,
                SelectorOperator.Equals => hasValue && values.Contains(value ?? string.Empty, StringComparer.Ordinal),
                SelectorOperator.NotEquals => !hasValue || !values.Contains(value ?? string.Empty, StringComparer.Ordinal),
                SelectorOperator.In => hasValue && values.Contains(value ?? string.Empty, StringComparer.Ordinal),
                SelectorOperator.NotIn => !hasValue || !values.Contains(value ?? string.Empty, StringComparer.Ordinal),
                _ => true,
            };
        });
    }

    private static bool MatchesFieldSelector(JsonObject resource, string selector)
    {
        return SplitSelector(selector).All(requirement =>
        {
            var (path, operation, values) = ParseSelectorRequirement(requirement);
            var node = ResolveFieldPath(resource, path);
            var value = node is JsonValue jsonValue && jsonValue.TryGetValue<string>(out var text)
                ? text
                : node?.ToJsonString();
            return operation switch
            {
                SelectorOperator.Exists => node is not null,
                SelectorOperator.DoesNotExist => node is null,
                SelectorOperator.Equals => node is not null && values.Contains(value ?? string.Empty, StringComparer.Ordinal),
                SelectorOperator.NotEquals => node is null || !values.Contains(value ?? string.Empty, StringComparer.Ordinal),
                SelectorOperator.In => node is not null && values.Contains(value ?? string.Empty, StringComparer.Ordinal),
                SelectorOperator.NotIn => node is null || !values.Contains(value ?? string.Empty, StringComparer.Ordinal),
                _ => true,
            };
        });
    }

    private static JsonNode? ResolveFieldPath(JsonObject resource, string path)
    {
        JsonNode? current = resource;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            current = current is JsonObject obj && obj.TryGetPropertyValue(segment, out var value) ? value : null;
            if (current is null)
            {
                return null;
            }
        }

        return current;
    }

    private static IEnumerable<string> SplitSelector(string selector)
    {
        var depth = 0;
        var start = 0;
        for (var index = 0; index < selector.Length; index++)
        {
            switch (selector[index])
            {
                case '(':
                    depth++;
                    break;
                case ')':
                    depth = Math.Max(0, depth - 1);
                    break;
                case ',' when depth == 0:
                    yield return selector[start..index].Trim();
                    start = index + 1;
                    break;
            }
        }

        if (start < selector.Length)
        {
            yield return selector[start..].Trim();
        }
    }

    private static (string Key, SelectorOperator Operator, string[] Values) ParseSelectorRequirement(string requirement)
    {
        requirement = requirement.Trim();
        if (requirement.StartsWith('!'))
        {
            return (requirement[1..].Trim(), SelectorOperator.DoesNotExist, []);
        }

        var notIn = requirement.IndexOf(" notin ", StringComparison.Ordinal);
        if (notIn >= 0)
        {
            return (requirement[..notIn].Trim(), SelectorOperator.NotIn, ParseSelectorValues(requirement[(notIn + 7)..]));
        }

        var inOperator = requirement.IndexOf(" in ", StringComparison.Ordinal);
        if (inOperator >= 0)
        {
            return (requirement[..inOperator].Trim(), SelectorOperator.In, ParseSelectorValues(requirement[(inOperator + 4)..]));
        }

        var notEquals = requirement.IndexOf("!=", StringComparison.Ordinal);
        if (notEquals >= 0)
        {
            return (requirement[..notEquals].Trim(), SelectorOperator.NotEquals, [requirement[(notEquals + 2)..].Trim()]);
        }

        var equals = requirement.IndexOf('=');
        if (equals >= 0)
        {
            var operatorLength = equals + 1 < requirement.Length && requirement[equals + 1] == '=' ? 2 : 1;
            return (requirement[..equals].Trim(), SelectorOperator.Equals, [requirement[(equals + operatorLength)..].Trim()]);
        }

        return (requirement, SelectorOperator.Exists, []);
    }

    private static string[] ParseSelectorValues(string value)
    {
        value = value.Trim();
        if (value.StartsWith('(') && value.EndsWith(')'))
        {
            value = value[1..^1];
        }

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private enum SelectorOperator
    {
        Exists,
        DoesNotExist,
        Equals,
        NotEquals,
        In,
        NotIn,
    }


    private JsonObject Discovery(bool core)
    {
        var grouped = _definitions.Values
            .Where(x => string.IsNullOrEmpty(x.Api.Group) == core)
            .GroupBy(x => x.Api.Group, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new JsonObject
            {
                ["metadata"] = new JsonObject { ["name"] = group.Key },
                ["versions"] = new JsonArray(group
                    .GroupBy(x => x.Api.ApiVersion, StringComparer.Ordinal)
                    .OrderBy(version => version.Key, StringComparer.Ordinal)
                    .Select(version => new JsonObject
                    {
                        ["version"] = version.Key,
                        ["resources"] = new JsonArray(version
                            .OrderBy(x => x.Api.PluralName, StringComparer.Ordinal)
                            .Select(CreateDiscoveredResource)
                            .ToArray()),
                    }).ToArray()),
            }).ToArray();

        return new JsonObject
        {
            ["apiVersion"] = "apidiscovery.k8s.io/v2beta1",
            ["kind"] = "APIGroupDiscoveryList",
            ["items"] = new JsonArray(grouped),
        };
    }

    private static JsonObject CreateDiscoveredResource(ResourceDefinition definition)
    {
        var resource = new JsonObject
        {
            ["resource"] = definition.Api.PluralName,
            ["responseKind"] = new JsonObject
            {
                ["group"] = definition.Api.Group,
                ["version"] = definition.Api.ApiVersion,
                ["kind"] = definition.Api.Kind,
            },
            ["scope"] = definition.Namespaced ? "Namespaced" : "Cluster",
            ["singularResource"] = definition.SingularName,
            ["verbs"] = new JsonArray("create", "delete", "get", "list", "patch", "update", "watch"),
        };

        if (definition.ShortNames.Count > 0)
        {
            resource["shortNames"] = new JsonArray(definition.ShortNames.Select(name => (JsonNode?)name).ToArray());
        }

        if (definition.Subresources.Count > 0)
        {
            resource["subresources"] = new JsonArray(definition.Subresources
                .Select(subresource => new JsonObject
                {
                    ["subresource"] = subresource.Name,
                    ["responseKind"] = new JsonObject
                    {
                        ["group"] = subresource.ResponseGroup ?? definition.Api.Group,
                        ["version"] = subresource.ResponseVersion ?? definition.Api.ApiVersion,
                        ["kind"] = subresource.ResponseKind ?? definition.Api.Kind,
                    },
                    ["verbs"] = new JsonArray(subresource.Verbs.Select(verb => (JsonNode?)verb).ToArray()),
                })
                .ToArray());
        }

        return resource;
    }

    private JsonObject ResourceList(string group, string version)
    {
        var entries = new List<JsonNode?>();
        foreach (var definition in _definitions.Values
            .Where(definition => definition.Api.Group == group && definition.Api.ApiVersion == version)
            .OrderBy(definition => definition.Api.PluralName, StringComparer.Ordinal))
        {
            entries.Add(new JsonObject
            {
                ["name"] = definition.Api.PluralName,
                ["singularName"] = definition.SingularName,
                ["namespaced"] = definition.Namespaced,
                ["kind"] = definition.Api.Kind,
                ["verbs"] = new JsonArray("create", "delete", "get", "list", "patch", "update", "watch"),
                ["shortNames"] = new JsonArray(definition.ShortNames.Select(name => (JsonNode?)name).ToArray()),
            });

            foreach (var subresource in definition.Subresources)
            {
                entries.Add(new JsonObject
                {
                    ["name"] = definition.Api.PluralName + "/" + subresource.Name,
                    ["singularName"] = string.Empty,
                    ["namespaced"] = definition.Namespaced,
                    ["kind"] = subresource.ResponseKind ?? definition.Api.Kind,
                    ["verbs"] = new JsonArray(subresource.Verbs.Select(verb => (JsonNode?)verb).ToArray()),
                });
            }
        }

        return new JsonObject
        {
            ["apiVersion"] = "v1",
            ["kind"] = "APIResourceList",
            ["groupVersion"] = string.IsNullOrEmpty(group) ? version : $"{group}/{version}",
            ["resources"] = new JsonArray(entries.ToArray()),
        };
    }

    private JsonObject ApiVersions()
    {
        return new JsonObject
        {
            ["kind"] = "APIVersions",
            ["versions"] = new JsonArray("v1"),
            ["serverAddressByClientCIDRs"] = new JsonArray(),
        };
    }

    private JsonObject? ApiGroup(string group)
    {
        var definitions = _definitions.Values
            .Where(definition => definition.Api.Group == group)
            .GroupBy(definition => definition.Api.ApiVersion, StringComparer.Ordinal)
            .OrderBy(version => version.Key, StringComparer.Ordinal)
            .Select(version => new JsonObject
            {
                ["groupVersion"] = $"{group}/{version.Key}",
                ["version"] = version.Key,
            })
            .ToArray();

        if (definitions.Length == 0)
        {
            return null;
        }

        return new JsonObject
        {
            ["apiVersion"] = "v1",
            ["kind"] = "APIGroup",
            ["name"] = group,
            ["versions"] = new JsonArray(definitions.Select(version => (JsonNode?)version).ToArray()),
            ["preferredVersion"] = definitions[0].DeepClone(),
        };
    }

    private JsonObject ApiGroupList()
    {
        var groups = _definitions.Values
            .Where(definition => !string.IsNullOrEmpty(definition.Api.Group))
            .GroupBy(definition => definition.Api.Group, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group =>
            {
                var versions = group
                    .GroupBy(definition => definition.Api.ApiVersion, StringComparer.Ordinal)
                    .OrderBy(version => version.Key, StringComparer.Ordinal)
                    .Select(version => new JsonObject
                    {
                        ["groupVersion"] = $"{group.Key}/{version.Key}",
                        ["version"] = version.Key,
                    })
                    .ToArray();

                return new JsonObject
                {
                    ["name"] = group.Key,
                    ["versions"] = new JsonArray(versions.Select(version => (JsonNode?)version).ToArray()),
                    ["preferredVersion"] = versions[0].DeepClone(),
                };
            })
            .ToArray();

        return new JsonObject
        {
            ["apiVersion"] = "v1",
            ["kind"] = "APIGroupList",
            ["groups"] = new JsonArray(groups.Select(group => (JsonNode?)group).ToArray()),
        };
    }

    private static bool TryGetGroupDiscoveryRoute(string path, out string group, out string? version)
    {
        group = string.Empty;
        version = null;
        const string prefix = "/apis/";
        if (!path.StartsWith(prefix, StringComparison.Ordinal))
        {
            return false;
        }

        var parts = path[prefix.Length..].Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length is < 1 or > 2)
        {
            return false;
        }

        group = parts[0];
        version = parts.Length == 2 ? parts[1] : null;
        return true;
    }

    private void RegisterCustomResourceDefinition(JsonObject resource)
    {
        var spec = resource["spec"]?.AsObject();
        var group = spec?["group"]?.GetValue<string>();
        var names = spec?["names"]?.AsObject();
        var plural = names?["plural"]?.GetValue<string>();
        var kind = names?["kind"]?.GetValue<string>();
        var singular = names?["singular"]?.GetValue<string>();
        var listKind = names?["listKind"]?.GetValue<string>();
        var shortNames = names?["shortNames"]?.AsArray()
            .Select(node => node?.GetValue<string>())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToArray() ?? [];
        var scope = spec?["scope"]?.GetValue<string>();
        var versions = spec?["versions"]?.AsArray();

        if (string.IsNullOrWhiteSpace(group)
            || string.IsNullOrWhiteSpace(plural)
            || string.IsNullOrWhiteSpace(kind)
            || versions is null)
        {
            return;
        }

        lock (_state.DiscoveryGate)
        {
            foreach (var versionNode in versions.OfType<JsonObject>())
            {
                var version = versionNode["name"]?.GetValue<string>();
                var served = versionNode["served"]?.GetValue<bool>() ?? true;
                if (string.IsNullOrWhiteSpace(version) || !served)
                {
                    continue;
                }

                var subresources = new List<ResourceSubresource>();
                var statusEnabled = versionNode["subresources"]?["status"] is not null;
                var scaleNode = versionNode["subresources"]?["scale"];
                if (statusEnabled)
                {
                    subresources.Add(StatusSubresource);
                }

                if (scaleNode is not null)
                {
                    subresources.Add(ScaleSubresource);
                }

                _definitions[DefinitionKey(group, version, plural)] = new ResourceDefinition(
                    new GroupApiVersionKind(group, version, kind, plural),
                    string.Equals(scope, "Namespaced", StringComparison.Ordinal),
                    listKind ?? kind + "List",
                    singular ?? InferSingular(plural),
                    shortNames,
                    subresources,
                    statusEnabled);
            }

            InvalidateGroupedDiscoveryETag();
        }
    }

    private void UnregisterCustomResourceDefinition(JsonObject resource)
    {
        var spec = resource["spec"]?.AsObject();
        var group = spec?["group"]?.GetValue<string>();
        var plural = spec?["names"]?["plural"]?.GetValue<string>();
        var versions = spec?["versions"]?.AsArray();
        if (string.IsNullOrWhiteSpace(group) || string.IsNullOrWhiteSpace(plural) || versions is null)
        {
            return;
        }

        lock (_state.DiscoveryGate)
        {
            var removed = false;
            foreach (var versionNode in versions.OfType<JsonObject>())
            {
                var version = versionNode["name"]?.GetValue<string>();
                if (string.IsNullOrWhiteSpace(version))
                {
                    continue;
                }

                var key = DefinitionKey(group, version, plural);
                removed |= _definitions.TryRemove(key, out _);

                var prefix = $"/apis/{group}/{version}/{plural}/";
                foreach (var resourceKey in _resources.Keys.Where(key => key.Contains(prefix, StringComparison.Ordinal)).ToArray())
                {
                    _resources.TryRemove(resourceKey, out _);
                }

                foreach (var collectionKey in _watchers.Keys.Where(key => key.Contains($"/{plural}", StringComparison.Ordinal)).ToArray())
                {
                    _watchers.TryRemove(collectionKey, out _);
                }
            }

            if (removed)
            {
                InvalidateGroupedDiscoveryETag();
            }
        }
    }

    private void InvalidateGroupedDiscoveryETag()
    {
        var revision = ++_state.GroupedDiscoveryRevision;
        _state.GroupedDiscoveryETag = $"\"fake-discovery-groups-v{revision}\"";
    }

    private void RegisterPolicyResources(JsonObject resource)
    {
        var kind = resource["kind"]?.GetValue<string>();
        if (kind is not ("Role" or "ClusterRole")
            || resource["rules"] is not JsonArray rules)
        {
            return;
        }

        foreach (var rule in rules.OfType<JsonObject>())
        {
            if (rule["resources"] is not JsonArray resources
                || rule["apiGroups"] is not JsonArray apiGroups)
            {
                continue;
            }

            foreach (var groupNode in apiGroups)
            {
                var group = groupNode?.GetValue<string>();
                if (group is null)
                {
                    continue;
                }

                if (group == "*" && resources.Any(resourceNode => resourceNode?.GetValue<string>() == "*"))
                {
                    RegisterPolicyResource(string.Empty, "pods");
                    RegisterPolicyResource("apps", "deployments");
                    continue;
                }

                foreach (var resourceNode in resources)
                {
                    var resourceName = resourceNode?.GetValue<string>()?.Split('/')[0];
                    if (string.IsNullOrWhiteSpace(resourceName) || resourceName == "*")
                    {
                        continue;
                    }

                    RegisterPolicyResource(group, resourceName);
                }
            }
        }
    }

    private void RegisterPolicyResource(string group, string resourceName)
    {
        const string apiVersion = "v1";
        var key = DefinitionKey(group, apiVersion, resourceName);
        _definitions.TryAdd(
            key,
            CreateDefinition(
                new GroupApiVersionKind(group, apiVersion, InferKind(resourceName), resourceName),
                IsNamespacedResource(resourceName)));
    }

    private static bool IsNamespacedResource(string resource)
        => resource is not ("namespaces"
            or "nodes"
            or "persistentvolumes"
            or "storageclasses"
            or "customresourcedefinitions"
            or "clusterroles"
            or "clusterrolebindings"
            or "ingressclasses"
            or "priorityclasses"
            or "runtimeclasses"
            or "validatingwebhookconfigurations"
            or "mutatingwebhookconfigurations"
            or "apiservices"
            or "certificatesigningrequests"
            or "tokenreviews"
            or "subjectaccessreviews"
            or "selfsubjectaccessreviews"
            or "selfsubjectrulesreviews"
            or "flowschemas"
            or "prioritylevelconfigurations"
            or "volumeattachments"
            or "csidrivers"
            or "csinodes"
            or "storageversions"
            or "podsecuritypolicies"
            or "ipaddresses"
            or "servicecidrs");

    private static bool AcceptsApiDiscovery(HttpRequestMessage request) =>
        request.Headers.Accept.Any(value => value.ToString().Contains("apidiscovery.k8s.io", StringComparison.OrdinalIgnoreCase));

    private bool IsAllowed(HttpRequestMessage request, Route route)
    {
        if (!_definitions.TryGetValue(DefinitionKey(route.Group, route.ApiVersion, route.PluralName), out _))
        {
            return true;
        }

        var verb = request.RequestUri?.Query.Contains("watch=true", StringComparison.OrdinalIgnoreCase) == true
            ? "watch"
            : request.Method.Method.ToUpperInvariant() switch
            {
                "GET" when route.ResourceName is null => "list",
                "GET" => "get",
                "POST" => "create",
                "PUT" => "update",
                "PATCH" => "patch",
                "DELETE" => "delete",
                _ => request.Method.Method.ToLowerInvariant(),
            };

        var @namespace = route.Namespace;
        var permissionKey = PermissionKey(route.PluralName, verb, @namespace, route.Subresource);
        return _permissions.TryGetValue(permissionKey, out var configured)
            ? configured
            : UseRoleBasedAuthorization
                ? IsRoleBasedAccessAllowed(route.Group, route.PluralName, verb, route.Namespace, route.Subresource, route.ResourceName)
                : true;
    }

    private static string InferKind(string pluralName)
    {
        var singularName = InferSingular(pluralName);
        return char.ToUpperInvariant(singularName[0]) + singularName[1..];
    }

    private static string InferSingular(string pluralName)
    {
        if (pluralName.EndsWith("sses", StringComparison.Ordinal))
        {
            return pluralName[..^2];
        }

        if (pluralName.EndsWith("ies", StringComparison.Ordinal))
        {
            return pluralName[..^3] + "y";
        }

        if (pluralName.EndsWith("ses", StringComparison.Ordinal)
            || pluralName.EndsWith("xes", StringComparison.Ordinal)
            || pluralName.EndsWith("zes", StringComparison.Ordinal)
            || pluralName.EndsWith("ches", StringComparison.Ordinal)
            || pluralName.EndsWith("shes", StringComparison.Ordinal))
        {
            return pluralName[..^2];
        }

        return pluralName.EndsWith('s') ? pluralName[..^1] : pluralName;
    }

    private bool IsRoleBasedAccessAllowed(string group, string resource, string verb, string? @namespace, string? subresource, string? resourceName = null)
    {
        var subject = ParseServiceAccount(AuthenticatedUser);
        if (subject is null)
        {
            return false;
        }

        foreach (var binding in _resources.Values)
        {
            if (string.Equals(binding["kind"]?.GetValue<string>(), "ClusterRoleBinding", StringComparison.Ordinal))
            {
                var clusterRoleBinding = binding.Deserialize<V1ClusterRoleBinding>();
                if (clusterRoleBinding is not null && IsSubjectMatch(clusterRoleBinding.Subjects, subject.Value))
                {
                    var role = GetClusterRole(clusterRoleBinding.RoleRef?.Name);
                    if (role is not null && RulesAllow(role.Rules, group, resource, verb, subresource, resourceName))
                    {
                        return true;
                    }
                }
            }
            else if (string.Equals(binding["kind"]?.GetValue<string>(), "RoleBinding", StringComparison.Ordinal))
            {
                var roleBinding = binding.Deserialize<V1RoleBinding>();
                if (roleBinding is null
                    || !string.Equals(roleBinding.Metadata?.NamespaceProperty, @namespace, StringComparison.Ordinal)
                    || !IsSubjectMatch(roleBinding.Subjects, subject.Value))
                {
                    continue;
                }

                var rules = string.Equals(roleBinding.RoleRef?.Kind, "ClusterRole", StringComparison.Ordinal)
                    ? GetClusterRole(roleBinding.RoleRef.Name)?.Rules
                    : GetRole(roleBinding.RoleRef?.Name, roleBinding.Metadata?.NamespaceProperty)?.Rules;
                if (RulesAllow(rules, group, resource, verb, subresource, resourceName))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private V1ClusterRole? GetClusterRole(string? name)
        => _resources.Values
            .Where(resource => string.Equals(resource["kind"]?.GetValue<string>(), "ClusterRole", StringComparison.Ordinal))
            .Select(resource => resource.Deserialize<V1ClusterRole>())
            .FirstOrDefault(role => string.Equals(role?.Name(), name, StringComparison.Ordinal));

    private V1Role? GetRole(string? name, string? @namespace)
        => _resources.Values
            .Where(resource => string.Equals(resource["kind"]?.GetValue<string>(), "Role", StringComparison.Ordinal))
            .Select(resource => resource.Deserialize<V1Role>())
            .FirstOrDefault(role => string.Equals(role?.Name(), name, StringComparison.Ordinal)
                && string.Equals(role.Namespace(), @namespace, StringComparison.Ordinal));

    private static bool IsSubjectMatch(IList<Rbacv1Subject>? subjects, (string Namespace, string Name) subject)
        => subjects?.Any(item => string.Equals(item.Kind, "ServiceAccount", StringComparison.Ordinal)
            && string.Equals(item.NamespaceProperty, subject.Namespace, StringComparison.Ordinal)
            && string.Equals(item.Name, subject.Name, StringComparison.Ordinal)) == true;

    private static bool RulesAllow(IList<V1PolicyRule>? rules, string group, string resource, string verb, string? subresource, string? resourceName = null)
    {
        var requestedResource = string.IsNullOrEmpty(subresource) ? resource : resource + "/" + subresource;
        return rules?.Any(rule => Matches(rule.ApiGroups, group)
            && Matches(rule.Resources, requestedResource)
            && Matches(rule.Verbs, verb)
            && (rule.ResourceNames is not { Count: > 0 }
                || (resourceName is not null && rule.ResourceNames.Contains(resourceName)))) == true;
    }

    private static bool Matches(IList<string>? values, string value)
        => values?.Any(item => item == "*" || string.Equals(item, value, StringComparison.Ordinal)) == true;

    private static bool MatchesNonResourceUrl(string rule, string path)
        => rule == "*"
            || string.Equals(rule, path, StringComparison.Ordinal)
            || (rule.EndsWith('*') && path.StartsWith(rule[..^1], StringComparison.Ordinal));

    private static (string Namespace, string Name)? ParseServiceAccount(string user)
    {
        const string prefix = "system:serviceaccount:";
        if (!user.StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        var parts = user[prefix.Length..].Split(':', 2);
        return parts.Length == 2 ? (parts[0], parts[1]) : null;
    }

    private static JsonObject ParseObject(string json) => JsonNode.Parse(json)?.AsObject() ?? throw new InvalidOperationException("Kubernetes JSON was not an object.");

    private static void NormalizeSecret(JsonObject resource)
    {
        if (resource["stringData"] is not JsonObject stringData)
        {
            return;
        }

        var data = resource["data"] as JsonObject ?? new JsonObject();
        foreach (var property in stringData)
        {
            if (property.Value is JsonValue value && value.TryGetValue<string>(out var plainText))
            {
                data[property.Key] = Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
            }
        }

        resource["data"] = data;
        resource.Remove("stringData");
    }

    private static void Merge(JsonObject target, JsonObject patch, bool strategic)
    {
        foreach (var property in patch)
        {
            if (property.Value is null)
            {
                target.Remove(property.Key);
            }
            else if (property.Value is JsonObject patchObject)
            {
                var targetObject = target[property.Key] as JsonObject ?? new JsonObject();
                target[property.Key] = targetObject;
                Merge(targetObject, patchObject, strategic);
            }
            else if (strategic
                && property.Value is JsonArray patchArray
                && target[property.Key] is JsonArray targetArray
                && TryMergeStrategicArray(targetArray, patchArray))
            {
                continue;
            }
            else
            {
                target[property.Key] = property.Value?.DeepClone();
            }
        }
    }

    private static bool TryMergeStrategicArray(JsonArray target, JsonArray patch)
    {
        var patchObjects = patch.OfType<JsonObject>().ToArray();
        if (patchObjects.Length != patch.Count || patchObjects.Any(item => item["name"] is null))
        {
            return false;
        }

        foreach (var patchItem in patchObjects)
        {
            var name = patchItem["name"]!.GetValue<string>();
            var existing = target
                .OfType<JsonObject>()
                .FirstOrDefault(item => string.Equals(item["name"]?.GetValue<string>(), name, StringComparison.Ordinal));
            if (existing is null)
            {
                target.Add(patchItem.DeepClone());
            }
            else
            {
                Merge(existing, patchItem, strategic: true);
            }
        }

        return true;
    }

    private void EnsureMetadata(JsonObject resource, V1ObjectMeta? metadata, bool isCreate = false)
    {
        var objectMetadata = resource["metadata"] as JsonObject ?? new JsonObject();
        if (isCreate)
        {
            objectMetadata["uid"] = Guid.NewGuid().ToString("N");
            objectMetadata["creationTimestamp"] = DateTime.UtcNow;
        }
        else
        {
            objectMetadata["uid"] ??= Guid.NewGuid().ToString("N");
            objectMetadata["creationTimestamp"] ??= metadata?.CreationTimestamp ?? DateTime.UtcNow;
        }

        objectMetadata["resourceVersion"] = CurrentResourceVersion();
        resource["metadata"] = objectMetadata;
    }

    private string CurrentResourceVersion() => Interlocked.Increment(ref _state.ResourceVersion).ToString(System.Globalization.CultureInfo.InvariantCulture);

    private string PeekResourceVersion() => Volatile.Read(ref _state.ResourceVersion).ToString(System.Globalization.CultureInfo.InvariantCulture);

    private HttpResponseMessage WatchResponse(string collectionKey, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var labelSelector = GetQueryValue(request, "labelSelector");
        var fieldSelector = GetQueryValue(request, "fieldSelector");
        var sendInitialEvents = string.Equals(GetQueryValue(request, "sendInitialEvents"), "true", StringComparison.OrdinalIgnoreCase);
        var allowBookmarks = sendInitialEvents
            || string.Equals(GetQueryValue(request, "allowWatchBookmarks"), "true", StringComparison.OrdinalIgnoreCase);
        var requestedVersion = GetQueryValue(request, "resourceVersion");
        var currentVersion = PeekResourceVersion();
        var initial = sendInitialEvents || string.IsNullOrEmpty(requestedVersion) || requestedVersion == "0";

        List<WatchEventRecord> replay = [];
        if (!initial)
        {
            if (!long.TryParse(requestedVersion, out var requestedRevision))
            {
                return BadRequestStatus("resourceVersion: Invalid value: the resource version must be an integer");
            }

            lock (_state.WatchHistoryGate)
            {
                if (_state.WatchHistory.Count > 0)
                {
                    var oldestRetained = _state.WatchHistory[0].ResourceVersion;
                    var currentRevision = long.Parse(currentVersion, System.Globalization.CultureInfo.InvariantCulture);
                    if (requestedRevision < oldestRetained - 1 && requestedRevision != currentRevision)
                    {
                        return Error(
                            HttpStatusCode.Gone,
                            "too old resource version: the requested resource version is no longer available; relist the collection");
                    }
                }

                replay.AddRange(_state.WatchHistory.Where(record => record.ResourceVersion > requestedRevision));
            }
        }

        var channel = Channel.CreateUnbounded<byte[]>();
        _watchers.GetOrAdd(collectionKey, static _ => []).Add(channel);

        if (initial)
        {
            foreach (var resource in _resources
                .Where(pair => IsResourceInCollection(pair.Key, collectionKey))
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => pair.Value)
                .Where(resource => MatchesSelectors(resource, labelSelector, fieldSelector)))
            {
                PublishWatch(channel, "ADDED", resource);
            }
        }
        else
        {
            foreach (var record in replay)
            {
                if (!IsWatchCollectionMatch(record.CollectionKey, collectionKey)
                    || !MatchesSelectors(record.Object, labelSelector, fieldSelector))
                {
                    continue;
                }

                PublishWatch(channel, record.Type, record.Object);
            }
        }

        if (allowBookmarks)
        {
            PublishWatch(channel, "BOOKMARK", new JsonObject
            {
                ["metadata"] = new JsonObject { ["resourceVersion"] = currentVersion },
            });
        }

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new WatchStream(channel, cancellationToken, _shutdownCancellation.Token)),
        };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return response;
    }

    private static bool MatchesSelectors(JsonObject resource, string? labelSelector, string? fieldSelector)
        => (string.IsNullOrWhiteSpace(labelSelector) || MatchesLabelSelector(resource, labelSelector))
            && (string.IsNullOrWhiteSpace(fieldSelector) || MatchesFieldSelector(resource, fieldSelector));

    private static bool IsWatchCollectionMatch(string eventCollectionKey, string watchCollectionKey)
    {
        if (string.Equals(eventCollectionKey, watchCollectionKey, StringComparison.Ordinal))
        {
            return true;
        }

        return string.Equals(GetGlobalCollectionKey(eventCollectionKey), watchCollectionKey, StringComparison.Ordinal);
    }

    private static void PublishWatch(Channel<byte[]> channel, string type, JsonObject resource)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(new JsonObject
        {
            ["type"] = type,
            ["object"] = resource.DeepClone(),
        });
        var line = new byte[payload.Length + 1];
        payload.CopyTo(line, 0);
        line[^1] = (byte)'\n';
        channel.Writer.TryWrite(line);
    }

    private void PublishWatch(string collectionKey, string type, JsonObject resource)
    {
        RecordWatchEvent(collectionKey, type, resource);
        PublishWatchers(collectionKey, type, resource);

        var globalCollectionKey = GetGlobalCollectionKey(collectionKey);
        if (globalCollectionKey is not null)
        {
            PublishWatchers(globalCollectionKey, type, resource);
        }
    }

    private void RecordWatchEvent(string collectionKey, string type, JsonObject resource)
    {
        var resourceVersion = resource["metadata"]?["resourceVersion"]?.GetValue<string>();
        if (!long.TryParse(resourceVersion, out var revision))
        {
            revision = long.Parse(PeekResourceVersion(), System.Globalization.CultureInfo.InvariantCulture);
        }

        lock (_state.WatchHistoryGate)
        {
            _state.WatchHistory.Add(new WatchEventRecord(revision, collectionKey, type, resource.DeepClone().AsObject()));
            if (_state.WatchHistory.Count > 1000)
            {
                _state.WatchHistory.RemoveRange(0, _state.WatchHistory.Count - 1000);
            }
        }
    }

    private void PublishWatchers(string collectionKey, string type, JsonObject resource)
    {
        if (_watchers.TryGetValue(collectionKey, out var watchers))
        {
            foreach (var watcher in watchers)
            {
                PublishWatch(watcher, type, resource);
            }
        }
    }

    private static bool IsResourceInCollection(string resourceKey, string collectionKey)
    {
        if (resourceKey.StartsWith(collectionKey + "/", StringComparison.Ordinal))
        {
            var remainder = resourceKey[(collectionKey.Length + 1)..];
            return remainder.Length > 0 && !remainder.Contains('/');
        }

        var separator = resourceKey.LastIndexOf('/');
        return separator > 0 && GetGlobalCollectionKey(resourceKey[..separator]) == collectionKey;
    }

    private static string? GetGlobalCollectionKey(string collectionKey)
    {
        var parts = collectionKey.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var namespaceIndex = Array.IndexOf(parts, "namespaces");
        if (namespaceIndex < 0 || parts.Length <= namespaceIndex + 2)
        {
            return null;
        }

        return string.Join('/', parts.Take(namespaceIndex)) + "/" + string.Join('/', parts.Skip(namespaceIndex + 2));
    }

    private sealed class WatchStream(
        Channel<byte[]> channel,
        CancellationToken cancellationToken,
        CancellationToken shutdownCancellation) : Stream
    {
        private byte[]? _buffer;
        private int _offset;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => 0;
        public override long Position { get => 0; set => throw new NotSupportedException(); }
        public override void Flush() => throw new NotSupportedException();
        public override int Read(byte[] buffer, int offset, int count) => ReadAsync(buffer.AsMemory(offset, count), cancellationToken).GetAwaiter().GetResult();
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, token, shutdownCancellation);
            using var shutdownRegistration = shutdownCancellation.Register(static state => ((Channel<byte[]>)state!).Writer.TryComplete(), channel);
            while (true)
            {
                if (_buffer is not null && _offset < _buffer.Length)
                {
                    var count = Math.Min(buffer.Length, _buffer.Length - _offset);
                    _buffer.AsMemory(_offset, count).CopyTo(buffer);
                    _offset += count;
                    return count;
                }

                try
                {
                    _buffer = await channel.Reader.ReadAsync(linked.Token).ConfigureAwait(false);
                }
                catch (ChannelClosedException)
                {
                    return 0;
                }

                _offset = 0;
            }
        }
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken token)
            => ReadAsync(buffer.AsMemory(offset, count), token).AsTask();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing)
        {
            channel.Writer.TryComplete();
            base.Dispose(disposing);
        }

    }

    private static HttpResponseMessage Json(object value, HttpStatusCode status = HttpStatusCode.OK) => new(status)
    {
        Content = JsonContent.Create(value),
    };

    private static HttpResponseMessage Text(string value) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(value, Encoding.UTF8, "text/plain"),
    };

    private static HttpResponseMessage Error(HttpStatusCode status, string message) => Json(new
    {
        apiVersion = "v1",
        kind = "Status",
        status = "Failure",
        message,
        reason = status switch
        {
            HttpStatusCode.NotFound => "NotFound",
            HttpStatusCode.Conflict => "Conflict",
            HttpStatusCode.Forbidden => "Forbidden",
            HttpStatusCode.Gone => "Expired",
            _ => "BadRequest",
        },
        code = (int)status,
    }, status);

    private static HttpResponseMessage BadRequestStatus(string message) => Error(HttpStatusCode.BadRequest, message);

    private static HttpResponseMessage ConflictStatus(string message) => Error(HttpStatusCode.Conflict, message);

    private static HttpResponseMessage NotFoundStatus(Route route)
        => Error(HttpStatusCode.NotFound, $"the server could not find the requested resource {route.CollectionPath}");

    private static HttpResponseMessage AlreadyExistsStatus(Route route, string name)
        => Json(new
        {
            apiVersion = "v1",
            kind = "Status",
            status = "Failure",
            message = $"{route.PluralName} \"{name}\" already exists",
            reason = "AlreadyExists",
            code = (int)HttpStatusCode.Conflict,
            details = new
            {
                name,
                group = route.Group,
                kind = route.PluralName,
            },
        }, HttpStatusCode.Conflict);

    private static HttpResponseMessage InvalidStatus(string field, string message, string reason)
        => Json(new
        {
            apiVersion = "v1",
            kind = "Status",
            status = "Failure",
            message = $"{field}: {message}",
            reason = "Invalid",
            code = (int)HttpStatusCode.UnprocessableEntity,
            details = new
            {
                causes = new[] { new { field, message, reason } },
            },
        }, HttpStatusCode.UnprocessableEntity);

    private static string DefinitionKey(string group, string version, string plural) => $"{group}/{version}/{plural}";

    private JsonObject OpenApiV3Index() => new()
    {
        ["paths"] = new JsonObject
        {
            ["/api/v1"] = new JsonObject
            {
                ["serverRelativeURL"] = $"/openapi/v3/api/v1?hash={_state.OpenApiV3DocumentHash}",
            },
        },
    };

    private static JsonObject OpenApiV3Document()
    {
        var schemas = LoadPodOpenApiSchemas();
        schemas["io.example.com.v1.Widget"] = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["apiVersion"] = new JsonObject { ["type"] = "string" },
                ["kind"] = new JsonObject { ["type"] = "string" },
                ["metadata"] = new JsonObject { ["$ref"] = "#/components/schemas/io.k8s.apimachinery.pkg.apis.meta.v1.ObjectMeta" },
                ["spec"] = new JsonObject
                {
                    ["type"] = "object",
                    ["description"] = "Custom resource schema",
                },
            },
        };

        return new JsonObject
        {
            ["openapi"] = "3.0.0",
            ["info"] = new JsonObject
            {
                ["title"] = "Fake Kubernetes API",
                ["version"] = "v1",
            },
            ["paths"] = new JsonObject(),
            ["components"] = new JsonObject
            {
                ["schemas"] = schemas,
            },
        };
    }

    private static JsonObject LoadPodOpenApiSchemas()
    {
        const string resourceName = "KubeUI.Testing.Kubernetes.Transport.Assets.pod-openapi-v1-schemas.json";
        using var stream = typeof(FakeKubernetesHttpApi).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded OpenAPI schema resource '{resourceName}' was not found.");
        return JsonNode.Parse(stream)?.AsObject()
            ?? throw new InvalidOperationException("Embedded Pod OpenAPI schemas could not be parsed.");
    }

    private static string PermissionKey(string resource, string verb, string? @namespace, string? subresource) => $"{resource}|{verb}|{@namespace}|{subresource}";

    private static string ResourceKey(string path) => path.Trim('/');

    private static string CollectionKey(Route route) => route.CollectionPath.Trim('/');

    private static string ResourcePath(GroupApiVersionKind api, string? @namespace, string? name)
    {
        var prefix = string.IsNullOrEmpty(api.Group) ? $"/api/{api.ApiVersion}" : $"/apis/{api.Group}/{api.ApiVersion}";
        var collection = string.IsNullOrEmpty(@namespace) ? $"{prefix}/{api.PluralName}" : $"{prefix}/namespaces/{@namespace}/{api.PluralName}";
        return string.IsNullOrEmpty(name) ? collection : collection + "/" + name;
    }

    private static bool IsNamespaced(Type type)
        => type != typeof(V1Namespace)
            && type != typeof(V1Node)
            && type != typeof(V1PersistentVolume)
            && type != typeof(V1StorageClass)
            && type != typeof(V1CustomResourceDefinition)
            && type != typeof(V1ClusterRole)
            && type != typeof(V1ClusterRoleBinding)
            && type != typeof(V1IngressClass)
            && type != typeof(V1PriorityClass)
            && type != typeof(V1RuntimeClass)
            && type != typeof(V1ValidatingWebhookConfiguration)
            && type != typeof(V1MutatingWebhookConfiguration);

    private static Route? ParseRoute(string path)
    {
        var parts = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3 || (parts[0] != "api" && parts[0] != "apis"))
        {
            return null;
        }

        var index = parts[0] == "api" ? 2 : 3;
        if (parts.Length <= index)
        {
            return null;
        }

        if (parts[0] == "api"
            && parts.Length == index + 2
            && parts[index] == "namespaces")
        {
            var namespaceApiVersion = parts[1];
            var namespaceCollectionPath = "/" + string.Join('/', parts.Take(index + 1));
            return new Route(string.Empty, "/api", namespaceApiVersion, "namespaces", namespaceCollectionPath, null, parts[index + 1], null);
        }

        var apiVersion = parts[1 + (parts[0] == "apis" ? 1 : 0)];
        var prefix = "/" + string.Join('/', parts.Take(index));
        var namespaceIndex = Array.IndexOf(parts, "namespaces", index);
        var resourceIndex = namespaceIndex >= index && parts.Length > namespaceIndex + 2
            ? namespaceIndex + 2
            : index;
        if (parts.Length <= resourceIndex)
        {
            return null;
        }

        var @namespace = namespaceIndex >= index && parts.Length > namespaceIndex + 1
            ? parts[namespaceIndex + 1]
            : null;
        var plural = parts[resourceIndex];
        var collectionPath = "/" + string.Join('/', parts.Take(resourceIndex + 1));
        var name = parts.Length > resourceIndex + 1 ? parts[resourceIndex + 1] : null;
        var subresource = parts.Length > resourceIndex + 2 ? parts[resourceIndex + 2] : null;
        var group = parts[0] == "apis" ? parts[1] : string.Empty;
        return new Route(group, prefix, apiVersion, plural, collectionPath, @namespace, name, subresource);
    }

    private readonly record struct ResourceDefinition(
        GroupApiVersionKind Api,
        bool Namespaced,
        string ListKind,
        string SingularName,
        IReadOnlyList<string> ShortNames,
        IReadOnlyList<ResourceSubresource> Subresources,
        bool HasStatusSubresource)
    {
        public bool SupportsScale => Subresources.Any(subresource => subresource.Name == "scale");
    }

    private readonly record struct ResourceSubresource(
        string Name,
        IReadOnlyList<string> Verbs,
        string? ResponseGroup = null,
        string? ResponseVersion = null,
        string? ResponseKind = null);

    private static ResourceDefinition CreateDefinition(GroupApiVersionKind api, bool namespaced)
    {
        var metadata = KnownResourceMetadata.TryGetValue((api.Group, api.PluralName), out var known)
            ? known
            : ResourceMetadata.Default;
        var singular = metadata.SingularName ?? InferSingular(api.PluralName);
        var kind = KindOverrides.TryGetValue((api.Group, api.PluralName), out var knownKind)
            ? knownKind
            : string.IsNullOrEmpty(api.Kind) ? InferKind(api.PluralName) : api.Kind;
        var listKind = metadata.ListKind ?? kind + "List";
        var subresources = metadata.Subresources ?? [];
        var normalizedApi = new GroupApiVersionKind(api.Group, api.ApiVersion, kind, api.PluralName);
        return new ResourceDefinition(
            normalizedApi,
            namespaced || metadata.ForceNamespaced,
            listKind,
            singular,
            metadata.ShortNames ?? [],
            subresources,
            subresources.Any(subresource => subresource.Name == "status"));
    }

    private sealed record ResourceMetadata(
        string? SingularName = null,
        string? ListKind = null,
        IReadOnlyList<string>? ShortNames = null,
        IReadOnlyList<ResourceSubresource>? Subresources = null,
        bool ForceNamespaced = false)
    {
        public static ResourceMetadata Default { get; } = new();
    }

    private static readonly ResourceSubresource StatusSubresource = new(
        "status",
        ["get", "patch", "update"]);

    private static readonly ResourceSubresource ScaleSubresource = new(
        "scale",
        ["get", "patch", "update"],
        "autoscaling",
        "v1",
        "Scale");

    private static readonly Dictionary<(string Group, string Plural), ResourceMetadata> KnownResourceMetadata = new()
    {
        [("", "namespaces")] = new("namespace", "NamespaceList", ["ns"], [StatusSubresource]),
        [("", "nodes")] = new("node", "NodeList", [], [StatusSubresource, new("proxy", ["get", "create", "delete", "patch", "update"]), new("stats", ["get"]), new("log", ["get"]), new("metrics", ["get"]), new("spec", ["get"])]),
        [("", "pods")] = new("pod", "PodList", ["po"], [
            StatusSubresource,
            new("log", ["get"]),
            new("exec", ["create"]),
            new("attach", ["create"]),
            new("portforward", ["create"]),
            new("proxy", ["get", "create", "delete", "patch", "update"]),
            new("binding", ["create"]),
            new("eviction", ["create"]),
            new("ephemeralcontainers", ["get", "patch", "update"]),
        ]),
        [("", "services")] = new("service", "ServiceList", ["svc"], [StatusSubresource, new("proxy", ["get", "create", "delete", "patch", "update"])]),
        [("", "replicationcontrollers")] = new("replicationcontroller", "ReplicationControllerList", ["rc"], [StatusSubresource, ScaleSubresource]),
        [("", "persistentvolumes")] = new("persistentvolume", "PersistentVolumeList", ["pv"], [StatusSubresource]),
        [("", "persistentvolumeclaims")] = new("persistentvolumeclaim", "PersistentVolumeClaimList", ["pvc"], [StatusSubresource]),
        [("", "resourcequotas")] = new("resourcequota", "ResourceQuotaList", ["quota"], [StatusSubresource]),
        [("", "serviceaccounts")] = new("serviceaccount", "ServiceAccountList", ["sa"], []),
        [("", "secrets")] = new("secret", "SecretList", [], []),
        [("", "configmaps")] = new("configmap", "ConfigMapList", ["cm"], []),
        [("", "endpoints")] = new("endpoints", "EndpointsList", ["ep"], []),
        [("", "events")] = new("event", "EventList", ["ev"], []),
        [("", "limitranges")] = new("limitrange", "LimitRangeList", ["limits"], []),
        [("apps", "deployments")] = new("deployment", "DeploymentList", ["deploy"], [StatusSubresource, ScaleSubresource]),
        [("apps", "daemonsets")] = new("daemonset", "DaemonSetList", ["ds"], [StatusSubresource]),
        [("apps", "replicasets")] = new("replicaset", "ReplicaSetList", ["rs"], [StatusSubresource, ScaleSubresource]),
        [("apps", "statefulsets")] = new("statefulset", "StatefulSetList", ["sts"], [StatusSubresource, ScaleSubresource]),
        [("apps", "controllerrevisions")] = new("controllerrevision", "ControllerRevisionList", [], []),
        [("batch", "jobs")] = new("job", "JobList", [], [StatusSubresource]),
        [("batch", "cronjobs")] = new("cronjob", "CronJobList", ["cj"], [StatusSubresource]),
        [("apiextensions.k8s.io", "customresourcedefinitions")] = new("customresourcedefinition", "CustomResourceDefinitionList", ["crd", "crds"], [StatusSubresource]),
        [("storage.k8s.io", "storageclasses")] = new("storageclass", "StorageClassList", ["sc"], []),
        [("networking.k8s.io", "ingresses")] = new("ingress", "IngressList", ["ing"], [StatusSubresource]),
        [("networking.k8s.io", "networkpolicies")] = new("networkpolicy", "NetworkPolicyList", ["netpol"], []),
        [("networking.k8s.io", "ingressclasses")] = new("ingressclass", "IngressClassList", [], []),
        [("policy", "poddisruptionbudgets")] = new("poddisruptionbudget", "PodDisruptionBudgetList", ["pdb"], [StatusSubresource]),
        [("scheduling.k8s.io", "priorityclasses")] = new("priorityclass", "PriorityClassList", ["pc"], []),
        [("node.k8s.io", "runtimeclasses")] = new("runtimeclass", "RuntimeClassList", [], []),
        [("coordination.k8s.io", "leases")] = new("lease", "LeaseList", [], []),
        [("autoscaling", "horizontalpodautoscalers")] = new("horizontalpodautoscaler", "HorizontalPodAutoscalerList", ["hpa"], [StatusSubresource]),
    };

    private static readonly Dictionary<(string Group, string Plural), string> KindOverrides = new()
    {
        [("", "configmaps")] = "ConfigMap",
        [("apiextensions.k8s.io", "customresourcedefinitions")] = "CustomResourceDefinition",
        [("", "serviceaccounts")] = "ServiceAccount",
        [("rbac.authorization.k8s.io", "rolebindings")] = "RoleBinding",
        [("rbac.authorization.k8s.io", "clusterroles")] = "ClusterRole",
        [("rbac.authorization.k8s.io", "clusterrolebindings")] = "ClusterRoleBinding",
        [("", "persistentvolumes")] = "PersistentVolume",
        [("", "persistentvolumeclaims")] = "PersistentVolumeClaim",
        [("policy", "poddisruptionbudgets")] = "PodDisruptionBudget",
        [("autoscaling", "horizontalpodautoscalers")] = "HorizontalPodAutoscaler",
        [("policy", "podsecuritypolicies")] = "PodSecurityPolicy",
        [("scheduling.k8s.io", "priorityclasses")] = "PriorityClass",
        [("node.k8s.io", "runtimeclasses")] = "RuntimeClass",
        [("networking.k8s.io", "ingressclasses")] = "IngressClass",
        [("admissionregistration.k8s.io", "validatingwebhookconfigurations")] = "ValidatingWebhookConfiguration",
        [("admissionregistration.k8s.io", "mutatingwebhookconfigurations")] = "MutatingWebhookConfiguration",
        [("storage.k8s.io", "storageclasses")] = "StorageClass",
        [("networking.k8s.io", "networkpolicies")] = "NetworkPolicy",
        [("", "resourcequotas")] = "ResourceQuota",
        [("", "limitranges")] = "LimitRange",
        [("apps", "controllerrevisions")] = "ControllerRevision",
        [("apps", "daemonsets")] = "DaemonSet",
        [("apps", "statefulsets")] = "StatefulSet",
        [("apps", "replicasets")] = "ReplicaSet",
        [("discovery.k8s.io", "endpointslices")] = "EndpointSlice",
        [("apiregistration.k8s.io", "apiservices")] = "APIService",
        [("storage.k8s.io", "csidrivers")] = "CSIDriver",
        [("storage.k8s.io", "csinodes")] = "CSINode",
        [("storage.k8s.io", "csistoragecapacities")] = "CSIStorageCapacity",
        [("certificates.k8s.io", "certificatesigningrequests")] = "CertificateSigningRequest",
        [("authentication.k8s.io", "tokenreviews")] = "TokenReview",
        [("authorization.k8s.io", "subjectaccessreviews")] = "SubjectAccessReview",
        [("authorization.k8s.io", "selfsubjectaccessreviews")] = "SelfSubjectAccessReview",
        [("authorization.k8s.io", "selfsubjectrulesreviews")] = "SelfSubjectRulesReview",
        [("flowcontrol.apiserver.k8s.io", "flowschemas")] = "FlowSchema",
        [("flowcontrol.apiserver.k8s.io", "prioritylevelconfigurations")] = "PriorityLevelConfiguration",
        [("storage.k8s.io", "volumeattachments")] = "VolumeAttachment",
        [("internal.apiserver.k8s.io", "storageversions")] = "StorageVersion",
        [("networking.k8s.io", "ipaddresses")] = "IPAddress",
        [("networking.k8s.io", "servicecidrs")] = "ServiceCIDR",
    };


    private sealed class BackendState
    {
        public object DiscoveryGate { get; } = new();
        public object WatchHistoryGate { get; } = new();
        public ConcurrentDictionary<string, ResourceDefinition> Definitions { get; } = new(StringComparer.Ordinal);
        public ConcurrentDictionary<string, JsonObject> Resources { get; } = new(StringComparer.Ordinal);
        public ConcurrentDictionary<string, ConcurrentBag<Channel<byte[]>>> Watchers { get; } = new(StringComparer.Ordinal);
        public List<WatchEventRecord> WatchHistory { get; } = [];
        public CancellationTokenSource ShutdownCancellation { get; } = new();
        public bool FailConnection { get; set; }
        public bool ThrowOnConnection { get; set; }
        public int OpenApiV3IndexFailuresRemaining;
        public int OpenApiV3IndexStatusCode;
        public int OpenApiV3DocumentStatusCode;
        public string OpenApiV3DocumentHash = "fake-v1";
        public int RequireAuthorizationForDiscovery;
        public string CoreDiscoveryETag = "\"fake-discovery-core-v1\"";
        public string GroupedDiscoveryETag = "\"fake-discovery-groups-v1\"";
        public long GroupedDiscoveryRevision = 1;
        public long ResourceVersion;
        public int ServiceIpCounter;
    }

    private readonly record struct WatchEventRecord(long ResourceVersion, string CollectionKey, string Type, JsonObject Object);

    private readonly record struct Route(string Group, string Prefix, string ApiVersion, string PluralName, string CollectionPath, string? Namespace, string? ResourceName, string? Subresource);
}
