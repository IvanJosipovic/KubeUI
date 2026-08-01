using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Channels;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Testing;

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
        : this(new BackendState())
    {
    }

    private FakeKubernetesHttpApi(BackendState state)
    {
        _state = state;
        _definitions = state.Definitions;
        _resources = state.Resources;
        _watchers = state.Watchers;
        _shutdownCancellation = state.ShutdownCancellation;
    }

    internal FakeKubernetesHttpApi CreateClient() => new(_state);

    public bool DefaultPermissionAllowed { get; set; } = true;

    public bool UseRoleBasedAuthorization { get; set; }

    public string AuthenticatedUser { get; set; } = "system:admin";

    public bool FailConnection
    {
        get => _state.FailConnection;
        set => _state.FailConnection = value;
    }

    /// <summary>
    /// Simulated round-trip latency applied to every HTTP request.
    /// </summary>
    public TimeSpan ResponseDelay
    {
        get => _state.ResponseDelay;
        set => _state.ResponseDelay = value;
    }

    public IReadOnlyList<Uri?> RequestUris => _requestUris.ToArray();

    private int _authorizationRequestCount;

    public int AuthorizationRequestCount => Volatile.Read(ref _authorizationRequestCount);

    private readonly ConcurrentQueue<Uri?> _requestUris = new();

    public void Register<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var api = GroupApiVersionKind.From<T>();
        _definitions[DefinitionKey(api.Group, api.ApiVersion, api.PluralName)] = new ResourceDefinition(api, IsNamespaced(typeof(T)));
    }

    public void Add<T>(T resource) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        Register<T>();
        var api = GroupApiVersionKind.From<T>();
        var json = ParseObject(KubernetesJson.Serialize(resource));
        NormalizeSecret(json);
        EnsureMetadata(json, resource.Metadata);
        _resources[ResourceKey(ResourcePath(api, resource.Metadata?.NamespaceProperty, resource.Metadata?.Name))] = json;
    }

    public void AddYaml(string yaml)
    {
        foreach (var resource in KubeUI.Kubernetes.Serialization.KubernetesYaml.LoadAllFromString(yaml))
        {
            var add = GetType()
                .GetMethod(nameof(Add))!
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

        foreach (ConcurrentBag<Channel<byte[]>> watchers in _watchers.Values)
        {
            foreach (Channel<byte[]> watcher in watchers)
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

        if (ResponseDelay > TimeSpan.Zero)
        {
            using var timer = new PeriodicTimer(ResponseDelay);
            await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false);
        }

        if (request.RequestUri is null)
        {
            return SetRequest(request, Error(HttpStatusCode.BadRequest, "Request URI is missing."));
        }

        var path = request.RequestUri.AbsolutePath.TrimEnd('/');
        if (FailConnection)
        {
            return SetRequest(request, Error(HttpStatusCode.ServiceUnavailable, "simulated connection failure"));
        }

        if (path.Contains("selfsubjectaccessreviews", StringComparison.Ordinal))
        {
            return SetRequest(request, await HandleAuthorizationAsync(request, cancellationToken).ConfigureAwait(false));
        }

        if (request.Method == HttpMethod.Get && (path is "/api" or "/apis"))
        {
            if (path == "/api")
            {
                return SetRequest(request, Json(Discovery(true)));
            }

            if (!AcceptsApiDiscovery(request))
            {
                return SetRequest(request, Json(new { apiVersion = "v1", kind = "APIGroupList", groups = Array.Empty<object>() }));
            }

            return SetRequest(request, Json(Discovery(false)));
        }

        if (request.Method == HttpMethod.Get && path == "/version")
        {
            return SetRequest(request, Json(new { gitVersion = "v1.0.0", major = "1", minor = "0", apiVersion = "v1", kind = "Info" }));
        }

        if (request.Method == HttpMethod.Get && path == "/api/v1")
        {
            return SetRequest(request, Json(ResourceList(string.Empty, "v1")));
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

    private async Task<HttpResponseMessage> HandleAuthorizationAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _authorizationRequestCount);
        var body = await request.Content!.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var root = JsonNode.Parse(body)?.AsObject();
        var attributes = root?["spec"]?["resourceAttributes"]?.AsObject();
        var resource = attributes?["resource"]?.GetValue<string>() ?? string.Empty;
        var verb = attributes?["verb"]?.GetValue<string>() ?? string.Empty;
        var group = attributes?["group"]?.GetValue<string>() ?? string.Empty;
        var @namespace = attributes?["namespace"]?.GetValue<string>();
        var subresource = attributes?["subresource"]?.GetValue<string>();
        @namespace = string.IsNullOrEmpty(@namespace) ? null : @namespace;
        subresource = string.IsNullOrEmpty(subresource) ? null : subresource;
        var allowed = _permissions.TryGetValue(PermissionKey(resource, verb, @namespace, subresource), out var configured)
            ? configured
            : UseRoleBasedAuthorization
                ? IsRoleBasedAccessAllowed(group, resource, verb, @namespace, subresource)
                : DefaultPermissionAllowed;

        return Json(new
        {
            apiVersion = "authorization.k8s.io/v1",
            kind = "SelfSubjectAccessReview",
            status = new { allowed },
        }, HttpStatusCode.Created);
    }

    private async Task<HttpResponseMessage> HandleResourceAsync(HttpRequestMessage request, Route route, CancellationToken cancellationToken)
    {
        if (!IsAllowed(request, route))
        {
            return Error(HttpStatusCode.Forbidden, $"forbidden: {request.Method} {route.CollectionPath}");
        }

        var collectionKey = CollectionKey(route);
        var key = ResourceKey(route.ResourceName is null ? collectionKey : collectionKey + "/" + route.ResourceName);

        if (request.Method == HttpMethod.Get)
        {
            if (request.RequestUri?.Query.Contains("watch=true", StringComparison.OrdinalIgnoreCase) == true)
            {
                return WatchResponse(collectionKey, cancellationToken);
            }

            if (route.ResourceName is null)
            {
                var items = _resources
                    .Where(pair => IsResourceInCollection(pair.Key, collectionKey))
                    .Select(pair => pair.Value.DeepClone())
                    .ToArray();

                return Json(new JsonObject
                {
                    ["apiVersion"] = route.ApiVersion,
                    ["kind"] = route.PluralName + "List",
                    ["metadata"] = new JsonObject { ["resourceVersion"] = CurrentResourceVersion() },
                    ["items"] = new JsonArray(items),
                });
            }

            return _resources.TryGetValue(key, out var resource)
                ? Json(resource)
                : Error(HttpStatusCode.NotFound, $"Resource was not found: {key}");
        }

        if (request.Method == HttpMethod.Post)
        {
            var resource = await ReadObjectAsync(request, cancellationToken).ConfigureAwait(false);
            var name = resource["metadata"]?["name"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(name))
            {
                return Error(HttpStatusCode.BadRequest, "Resource metadata.name is required.");
            }

            if (request.RequestUri?.Query.Contains("dryRun=All", StringComparison.OrdinalIgnoreCase) == true
                && string.Equals(resource["kind"]?.GetValue<string>(), "Pod", StringComparison.Ordinal)
                && resource["spec"]?["containers"] is not JsonArray { Count: > 0 })
            {
                return Error(HttpStatusCode.UnprocessableEntity, "Pod spec.containers must contain at least one container.");
            }

            EnsureMetadata(resource, resource["metadata"]?.Deserialize<V1ObjectMeta>());
            NormalizeSecret(resource);
            _resources[ResourceKey(collectionKey + "/" + name)] = resource;
            if (route.PluralName == "customresourcedefinitions")
            {
                RegisterCustomResourceDefinition(resource);
            }
            PublishWatch(collectionKey, "ADDED", resource);
            return Json(resource, HttpStatusCode.Created);
        }

        if (request.Method == HttpMethod.Put || request.Method == HttpMethod.Patch)
        {
            var resource = await ReadObjectAsync(request, cancellationToken).ConfigureAwait(false);
            var existed = _resources.TryGetValue(key, out var existingResource);
            if (route.ResourceName is null)
            {
                return Error(HttpStatusCode.BadRequest, "A resource name is required for updates.");
            }

            if (existed && !string.Equals(
                    resource["metadata"]?["resourceVersion"]?.GetValue<string>(),
                    existingResource!["metadata"]?["resourceVersion"]?.GetValue<string>(),
                    StringComparison.Ordinal))
            {
                return Error(HttpStatusCode.Conflict, "The resourceVersion is stale.");
            }

            if (request.Method == HttpMethod.Patch && existingResource is not null)
            {
                Merge(existingResource, resource);
                resource = existingResource;
            }

            EnsureMetadata(resource, resource["metadata"]?.Deserialize<V1ObjectMeta>());
            NormalizeSecret(resource);
            _resources[key] = resource;
            PublishWatch(collectionKey, existed ? "MODIFIED" : "ADDED", resource);
            return Json(resource);
        }

        if (request.Method == HttpMethod.Delete)
        {
            if (route.ResourceName is null)
            {
                return Error(HttpStatusCode.BadRequest, "A resource name is required for deletion.");
            }

            _resources.TryRemove(key, out var deleted);
            if (deleted is not null)
            {
                PublishWatch(collectionKey, "DELETED", deleted);
            }
            return Json(deleted ?? new JsonObject { ["kind"] = "Status", ["status"] = "Success" });
        }

        return Error(HttpStatusCode.MethodNotAllowed, $"Unsupported method: {request.Method}");
    }

    private JsonObject Discovery(bool core)
    {
        var grouped = _definitions.Values
            .Where(x => string.IsNullOrEmpty(x.Api.Group) == core)
            .GroupBy(x => x.Api.Group, StringComparer.Ordinal)
            .Select(group => new JsonObject
            {
                ["metadata"] = new JsonObject { ["name"] = group.Key },
                ["versions"] = new JsonArray(group.Select(definition => new JsonObject
                {
                    ["version"] = definition.Api.ApiVersion,
                    ["resources"] = new JsonArray(group
                        .Where(x => x.Api.ApiVersion == definition.Api.ApiVersion)
                        .Select(x => new JsonObject
                        {
                            ["resource"] = x.Api.PluralName,
                             ["responseKind"] = new JsonObject { ["kind"] = x.Api.Kind },
                            ["scope"] = x.Namespaced ? "Namespaced" : "Cluster",
                            ["verbs"] = new JsonArray("create", "delete", "get", "list", "patch", "update", "watch"),
                        }).ToArray()),
                }).ToArray()),
            }).ToArray();

        return new JsonObject
        {
            ["apiVersion"] = "apidiscovery.k8s.io/v2beta1",
            ["kind"] = "APIGroupDiscoveryList",
            ["items"] = new JsonArray(grouped),
        };
    }

    private JsonObject ResourceList(string group, string version)
    {
        var definitions = _definitions.Values
            .Where(definition => definition.Api.Group == group && definition.Api.ApiVersion == version)
            .Select(definition => new JsonObject
            {
                ["name"] = definition.Api.PluralName,
                ["singularName"] = definition.Api.PluralName.TrimEnd('s'),
                ["namespaced"] = definition.Namespaced,
                ["kind"] = definition.Api.Kind,
                ["verbs"] = new JsonArray("create", "delete", "get", "list", "patch", "update", "watch"),
            })
            .ToArray();

        return new JsonObject
        {
            ["apiVersion"] = "v1",
            ["kind"] = "APIResourceList",
            ["groupVersion"] = string.IsNullOrEmpty(group) ? version : $"{group}/{version}",
            ["resources"] = new JsonArray(definitions),
        };
    }

    private void RegisterCustomResourceDefinition(JsonObject resource)
    {
        var spec = resource["spec"]?.AsObject();
        var group = spec?["group"]?.GetValue<string>();
        var names = spec?["names"]?.AsObject();
        var plural = names?["plural"]?.GetValue<string>();
        var kind = names?["kind"]?.GetValue<string>();
        var scope = spec?["scope"]?.GetValue<string>();
        var version = spec?["versions"]?.AsArray().FirstOrDefault()?["name"]?.GetValue<string>();

        if (!string.IsNullOrWhiteSpace(group)
            && !string.IsNullOrWhiteSpace(version)
            && !string.IsNullOrWhiteSpace(plural)
            && !string.IsNullOrWhiteSpace(kind))
        {
            _definitions[DefinitionKey(group, version, plural)] = new ResourceDefinition(
                new GroupApiVersionKind(group, version, kind, plural),
                string.Equals(scope, "Namespaced", StringComparison.Ordinal));
        }
    }

    private static bool AcceptsApiDiscovery(HttpRequestMessage request) =>
        request.Headers.Accept.Any(value => value.ToString().Contains("apidiscovery.k8s.io", StringComparison.OrdinalIgnoreCase));

    private bool IsAllowed(HttpRequestMessage request, Route route)
    {
        if (!_definitions.TryGetValue(DefinitionKey(route.Group, route.ApiVersion, route.PluralName), out var definition))
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

        var @namespace = definition.Namespaced ? route.Namespace : null;
        var permissionKey = PermissionKey(route.PluralName, verb, @namespace, route.Subresource);
        return _permissions.TryGetValue(permissionKey, out var configured)
            ? configured
            : UseRoleBasedAuthorization
                ? IsRoleBasedAccessAllowed(route.Group, route.PluralName, verb, route.Namespace, route.Subresource)
                : DefaultPermissionAllowed;
    }

    private bool IsRoleBasedAccessAllowed(string group, string resource, string verb, string? @namespace, string? subresource)
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
                    if (role is not null && RulesAllow(role.Rules, group, resource, verb, subresource))
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
                if (RulesAllow(rules, group, resource, verb, subresource))
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

    private static bool RulesAllow(IList<V1PolicyRule>? rules, string group, string resource, string verb, string? subresource)
    {
        var requestedResource = string.IsNullOrEmpty(subresource) ? resource : resource + "/" + subresource;
        return rules?.Any(rule => Matches(rule.ApiGroups, group)
            && Matches(rule.Resources, requestedResource)
            && Matches(rule.Verbs, verb)) == true;
    }

    private static bool Matches(IList<string>? values, string value)
        => values?.Any(item => item == "*" || string.Equals(item, value, StringComparison.Ordinal)) == true;

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

    private static async Task<JsonObject> ReadObjectAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return ParseObject(await request.Content!.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
    }

    private static void Merge(JsonObject target, JsonObject patch)
    {
        foreach (var property in patch)
        {
            if (property.Value is JsonObject patchObject && target[property.Key] is JsonObject targetObject)
            {
                Merge(targetObject, patchObject);
            }
            else
            {
                target[property.Key] = property.Value?.DeepClone();
            }
        }
    }

    private void EnsureMetadata(JsonObject resource, V1ObjectMeta? metadata)
    {
        var objectMetadata = resource["metadata"] as JsonObject ?? new JsonObject();
        objectMetadata["uid"] ??= Guid.NewGuid().ToString("N");
        objectMetadata["resourceVersion"] = CurrentResourceVersion();
        resource["metadata"] = objectMetadata;
        _ = metadata;
    }

    private string CurrentResourceVersion() => Interlocked.Increment(ref _state.ResourceVersion).ToString(System.Globalization.CultureInfo.InvariantCulture);

    private HttpResponseMessage WatchResponse(string collectionKey, CancellationToken cancellationToken)
    {
        var channel = Channel.CreateUnbounded<byte[]>();
        _watchers.GetOrAdd(collectionKey, static _ => []).Add(channel);
        PublishWatch(channel, "BOOKMARK", new JsonObject { ["metadata"] = new JsonObject { ["resourceVersion"] = "0" } });
        foreach (var resource in _resources
            .Where(pair => IsResourceInCollection(pair.Key, collectionKey))
            .Select(pair => pair.Value))
        {
            PublishWatch(channel, "ADDED", resource);
        }
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new WatchStream(channel, cancellationToken, _shutdownCancellation.Token)),
        };
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        return response;
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
        PublishWatchers(collectionKey, type, resource);

        var globalCollectionKey = GetGlobalCollectionKey(collectionKey);
        if (globalCollectionKey is not null)
        {
            PublishWatchers(globalCollectionKey, type, resource);
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
            return true;
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

    private static HttpResponseMessage Error(HttpStatusCode status, string message) => Json(new { kind = "Status", status = "Failure", message, code = (int)status }, status);

    private static string DefinitionKey(string group, string version, string plural) => $"{group}/{version}/{plural}";

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

    private readonly record struct ResourceDefinition(GroupApiVersionKind Api, bool Namespaced);

    private sealed class BackendState
    {
        public ConcurrentDictionary<string, ResourceDefinition> Definitions { get; } = new(StringComparer.Ordinal);
        public ConcurrentDictionary<string, JsonObject> Resources { get; } = new(StringComparer.Ordinal);
        public ConcurrentDictionary<string, ConcurrentBag<Channel<byte[]>>> Watchers { get; } = new(StringComparer.Ordinal);
        public CancellationTokenSource ShutdownCancellation { get; } = new();
        public TimeSpan ResponseDelay { get; set; } = TimeSpan.FromMilliseconds(50);
        public bool FailConnection { get; set; }
        public long ResourceVersion;
    }

    private readonly record struct Route(string Group, string Prefix, string ApiVersion, string PluralName, string CollectionPath, string? Namespace, string? ResourceName, string? Subresource);
}
