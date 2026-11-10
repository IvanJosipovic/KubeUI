# Test Suite Review — Plan

Date: 2026-11-04. Scope: all projects under `tests\` against rules in `tests\KubeUI.Testing\AGENTS.md`, `tests\KubeUI.Kubernetes.Tests\AGENTS.md`, `tests\KubeUI.App.Tests\AGENTS.md`, root AGENTS.md §6.

## Status: ALL ACTION ITEMS COMPLETE (2026-11-04)

Full-suite verification after all fixes (dotnet test, all 11 projects):

| Project | Result |
|---|---|
| Kubernetes.Tests | 337/337 passed |
| Avalonia.Tests | 375/375 passed |
| Clusters.Tests | 51/51 passed |
| Resources.Tests | 609/609 passed |
| Helm.Tests | 89 passed, 3 skipped (opt-in live) |
| Cilium.Tests | 58 passed, 1 skipped (opt-in live) |
| Crossplane.Tests | 17/17 passed |
| GatewayApi.Tests | 4/4 passed |
| AI.Tests | 103/103 passed |
| Azure.Tests | 4/4 passed |
| Documentation.Tests | 3 passed, 6 skipped (opt-in recording) |

### Item completion notes

1. **Duplicate suite deleted.** Removed `tests\KubeUI.Kubernetes.Tests\PortForwarderTests.cs` + `ClusterScenarioAssertions.cs`; only `Resources\PortForwarding\PortForwarderTests.cs` + `Clusters\Runtime\ClusterRuntimeAssertions.cs` remain. Build green.
2. **Banned mocks removed (23 sites).** McpToolsTests pod-log tests ported to `TestClusterGenerator` + transport fake (fake `/log` subresource now serves pod annotation `kubeui.testing/log`); all McpToolsTests/SettingsViewModelTests cluster mocks replaced with real generated clusters via `Infra/NamedClusterCatalogFactory.cs`; Crossplane suite fully ported off `Mock<IClusterRuntime>` to production Cluster + generated fixtures (`CrossplaneTestCluster.cs`); Cilium `HubbleConnectionTests` ported to real clusters. Moq removed from `KubeUI.Testing.csproj`, referenced directly by the projects that use it.
3. **TestCluster disposal.** `TestApp.RegisterTestCluster` + explicit dispose in `DisposeCoreAsync`; the seeded catalog-factory cluster registered too. TestCluster.DisposeAsync is idempotent, so generator cleanup stays safe.
4. **Fake transport contract fixed.** Atomic create (concurrent loser gets 409), lowercase RFC 1123 name validation, RBAC policy resources registered at real group versions (`PolicyGroupVersions` map), fake-path generator failure cleanup, fake admin client condition-handler parity with Kind.
5. **Config reset.** `TestClusterConfig.Reset()` + `CreateClusterAsync` resets before applying the lambda. Regression tests added in `TestClusterGeneratorTests` + `ApplicationServiceResolutionTests`. `TestApp.Notification` is per-app-instance state; fresh app per `[AvaloniaFact]` makes the ordering hazard moot.
6. **Timing asserts rewritten.** `connect_returns_before_synchronous_connection_work_completes` (observable `IsCompleted` gate), `selecting_cluster_node_does_not_block_on_slow_client_connection` (500 ms latency + status-not-connected), 20-iteration loop → bounded publish-until-observed with CT, `Thread.Join` bounded, `NavigationTreeBuilderTests` stopwatch + O(n²) removed (reference-identity via dictionary), `PermissionRefreshCoordinatorTests` bounded negative window, MR diff `Observable.Timer(100ms)`/750 ms windows replaced with bounded predicates.
7. **Dispose pass.** ~90 `using var` added across NavigationViewModelTests (63), ResourceYamlViewModelTests (53), ClusterWorkspaceTests (24); window `try/finally` in HostWindowStyleTests, NavigationViewTests, ClusterViewTests, ThemedSvgImageTests, ResourcePropertiesViewTests, ResourcePropertiesViewThreadingTests; PodDebugContainerTests workspace disposed. Root-cause fix found during the sweep: `PodLogsViewModel.Dispose` threw `TaskCanceledException` when disposing on a background thread during dispatcher shutdown — now tolerated (documented; exercised by the full PodLogsViewModelTests run since that state only exists at teardown).
8. **Matrices added.** Crossplane `GetProviderPodKeys`/`GetProviderContainerRestartCounts` now `[KubernetesBackendData]` + `[Trait("Category","Kind")]`; Cilium `IsCiliumInstalled` dot-boundary negative test added.
9. **Coverage filled.** New: `KubernetesOpenApiSchemaLoaderTests` (3 tests: load, retry-and-quiet-failure, skip-after-success), `TerminalHandlerTests` (6), `FileSystemHandlerTests` (8), `ExecutableLocatorTests` (3), `DenyByDefaultAgentPermissionServiceTests` (2), `PortForwarderListViewModelTests` (3), `ServiceMapViewModelFlowTests` (2, scripted Hubble connection), Cilium negative CRD-group test. **Documented as not actionable:** docking layout persistence round-trip (no disk layout persistence exists in the app — in-session restore is covered by `ResourceListViewRestoreTests`); AKS discovery/credential paths (ARM SDK has no test seam; would require a behavior-preserving production refactor — flagged for a future decision); ACP `Agent`/`AgentSession` external-process transport (AGENTS requires fake ACP tests to remain external-process free; needs a transport seam in production first).
10. **Hygiene.** Deleted 0-byte `GlobalUsings.GenericKubernetesObject.cs` (Kubernetes.Tests); ClusterManagerTests temp kubeconfigs now use `TemporaryDirectory`; mixed xUnit asserts converted to Shouldly in Clusters/Crossplane/Resources/Helm live test; pointless `[Obsolete]` removed from 6 documentation recording tests; duplicate tautological asserts removed (NavigationViewModelTests, NavigationSectionTests); `TestWait.UntilValueAsync` timeout contract documented in XML docs; `LiveHelmVisualizationTests` diagnostic NRE fixed (`Graph?.Resources.Count`) and `Assert.Fail` → `ShouldAssertException`.

### Incidents found and fixed while working

- `NavigationViewModelTests` fixture had an uppercase resource name (`event-from-decorated-workspace.Runtime`) that real API servers reject — fixture corrected to lowercase.
- Crossplane VM tests previously asserted `ClusterWorkspace`-mock-only behavior; the ported versions now verify real informer visibility, server-assigned uids, and pod **status subresource** semantics (regular update preserves status — fixtures now use `/status`).

## Original review content

## Verdict

Discipline is high: zero `Task.Delay`/`Thread.Sleep` (grep-verified), predicate-based waits, cancellation tokens threaded, fake/Kind backend matrix, `[LiveClusterFact]` instead of `Explicit`. Main defects: 4 zones of banned `Mock<IKubernetes>`/`Mock<IClusterRuntime>`, one dead duplicated suite (~1,800 lines), shared-state leaks inside test apps, several fake-transport contract bugs, uneven coverage between feature projects.

Non-finding: `VisualizationIncrementalUpdateCoordinator.cs` build error reported during review does not reproduce — full solution build succeeds, 0 errors.

## Approach map

Test pyramid: unit → fake transport (production Cluster + `FakeKubernetesHttpApi`) → fake/Kind matrix (`[KubernetesBackendData]`, `KUBEUI_RUN_KIND_TESTS=1`) → opt-in live (`[LiveClusterFact]` + `KUBEUI_LIVE_CONTEXT`) → docs recording (`KUBEUI_DOCS_VIDEO_DIR`).

| Project | Tests (≈) | Style | Consistent |
|---|---|---|---|
| Kubernetes.Tests | ~450 | matrix + unit, production client | Yes |
| Avalonia.Tests | ~350 | headless UI + fake + 1 matrix theory | Yes, 1 mock violation |
| Clusters.Tests | 51 | fake + matrix | Yes |
| Resources.Tests | 554 | fake + matrix + unit | Yes |
| GatewayApi.Tests | 4 | fake via test-owned workspace | Yes |
| Cilium.Tests | 51 | fake + 1 live | Mostly, 1 mock |
| AI.Tests | 32 | unit + headless | Yes |
| Helm.Tests | ~40 | fake + opt-in live | Yes |
| Crossplane.Tests | 17 | all Moq `IClusterRuntime`, no production cluster, no matrix | **No — outlier** |
| Azure.Tests | 4 | pure unit, no `KubeUI.Testing` | Intentional |
| Documentation.Tests | 3 + 12 opt-in | recording harness | Yes, by design |

Two styles side by side: production-cluster style (everything else) and mock style (Crossplane). Crossplane asserts seeding/informer/pod-cache only against Moq setups (`MRDiffDetectionViewModelTests.cs:107-113`); feature cluster path unverified by fake or Kind.

## High findings

1. **Duplicate `PortForwarderTests` suite + dead helper.**
   - `tests\KubeUI.Kubernetes.Tests\PortForwarderTests.cs` (ns `KubeUI.Kubernetes.Tests`) duplicates `tests\KubeUI.Kubernetes.Tests\Resources\PortForwarding\PortForwarderTests.cs` (ns `...Resources.PortForwarding`) — same class name, different namespaces, both compile, 13 shared tests run twice.
   - Root copy stale: missing `Service_forward_resolves_single_unnamed_target_port` (nested file line 223).
   - Root copy keeps dead 651-line `ClusterScenarioAssertions.cs` alive (only live refs are those 5 calls). Canonical helper: `Clusters\Runtime\ClusterRuntimeAssertions.cs`; already diverged (`ShouldBe(1)` vs `ShouldBeGreaterThanOrEqualTo(1)` in LimitedAccessCore).
2. **`Mock<IKubernetes>` / `Mock<IClusterRuntime>` where banned.** 23 sites:
   - `Avalonia.Tests\App\Infrastructure\Mcp\McpToolsTests.cs:592-651,713-720` — port to transport fake.
   - `Features.Crossplane.Tests` — entire cluster path (`CrossplaneProviderLogMonitorTests.cs:24,42,64...`, `MRDiffDetectionViewModelTests.cs:66,313,359,413`).
   - `Features.Cilium.Tests\HubbleConnectionTests.cs:14,24,41`.
   - `Avalonia.Tests\...\SettingsViewModelTests.cs:241-365` — name-only mocks.
   - Moq ships from `tests\KubeUI.Testing\KubeUI.Testing.csproj` into every test project; infra does not use it.
3. **Shared harness never disposes `TestCluster`.** `Avalonia.Tests\Infra\TestApplicationExtensions.cs:41-46` (compiled into all feature projects via `FeatureTests.props`) returns only the `ClusterWorkspace`; `TestCluster` lives in the generator singleton until process exit. `ClusterWorkspace.Dispose()` does not disconnect runtime or remove from `ClusterManager`/catalog. Connected clusters keep live informers for the rest of the run. Correct pattern exists in `KubeUIWalkthroughRecorder.cs:129`.
4. **Fake transport contract bugs** (`FakeKubernetesHttpApi.cs`):
   - Create not atomic: `ContainsKey` then indexer (`:835-852`) — concurrent creates both succeed instead of 409 (TOCTOU).
   - `IsDnsSubdomain` (`:1998-2004`) accepts uppercase names.
   - `RegisterPolicyResource` (`:2534-2543`) hardcodes `apiVersion = "v1"` for every group (triggered by `KubernetesScenarioData.cs:222-228`).
   - Fake generator failure path has no cleanup (`TestClusterGenerator.cs:185-205`); Kind/Live paths do.
   - Fake `AdminClient` skips condition handlers (`:191-194`) vs Kind (`:318-322`) — backend divergence.
5. **No fake/Kind matrix in any feature project.** `[KubernetesBackendData]` appears in 0 of the feature projects. Crossplane diff detection and Cilium extension logic are cluster logic without Kind cross-check.

## Medium findings

### Consistency
- `TestClusterConfig` DI singleton mutated in place; `CreateClusterAsync` only layers lambda (`TestApplicationExtensions.cs:34-42`), only `config.Name` reset. Fresh app per `[AvaloniaFact]` limits cross-test leak, but within-test multi-cluster creation inherits leftovers; correctness depends on undocumented app lifecycle. Only resets: `NavigationViewModelTests.cs:1258-1295`, `DataDisplayTests.cs:671`.
- `TestApp.Notification` cleared only at dispose — `ClusterListViewModelTests.cs:45` satisfiable by earlier test in same app; `ResourceFeatureConfigTests.cs:220` ordering-dependent.
- Singleton pollution inside one app run: `ImportAksClusterViewModelTests.cs:30-37` (catalog), `PodAttachTests.cs:186-218` (renames runtime, swaps client), `YamlHoverToolTipBehaviorTests.cs:43-52` (ModelCatalog schema).
- Fixture duplication: `Pod(...)`/`NamespaceResource` in ≥6 Resources files; `StubHttpMessageHandler` ×2 (`MainViewModelTests.cs:100-110`, `UpdateAvailableViewModelTests.cs:331-341`); `CapturingLoggerProvider` ×2; find-link recursion ×3; Crossplane fake stream families ×2 (`CrossplaneProviderLogMonitorTests.cs:285-424`, `MRDiffDetectionViewModelTests.cs:576-707`). Belong in `Infra/` or `KubeUI.Testing`.
- `NavigationViewModelTests.cs:2768-3078` defines 8 fakes inline in a 3,078-line file; shared setup belongs in `Infra/`.

### Bad logic / weak tests
- `ResourcePropertiesViewTests.cs:218-261` — test named `resource_updates_raise_object_changed...` subscribes to nothing.
- `ClusterWorkspaceTests.cs:684-696` — name says "skips initialization", asserts only `Connected.ShouldBeFalse()`.
- `ClusterWorkspaceTests.cs:654-669` — `stopwatch.Elapsed < 150ms` vs 200 ms latency budget; CI-flake. Same class: `NavigationViewModelTests.cs:1462-1475`, `:718-723`, `NavigationTreeBuilderTests.cs:128-137` (O(n²) 6,000-link compare inside timed assert).
- `MRDiffDetectionViewModelTests.cs:570-574` — `Observable.Timer(100ms).FirstAsync()` sleep substitute; negative asserts at `:269`, `:294` are timing windows.
- `PermissionRefreshCoordinatorTests.cs:93-110` — instant `passes.ShouldBe(0)`; vacuous pass.
- `ClusterWorkspaceTests.cs:66-67` — expected obtained by calling method under test; proves call stability, not lookup.
- `CrossplaneProviderLogMonitorTests.cs:79` — `OpenCount >= 3` weak lower bound.
- `KubeUIKubernetesServiceCollectionExtensionsTests.cs:12-17` — existence-only test.

### Rule violations
- Unbounded awaits: `VisualizationViewModelTests.cs:336-374`, `HubbleConnectionTests.cs:32,47`, `HelmEditorViewModelTests.cs:636,672`, `NavigationViewModelTests.cs:1366`, `LiveClusterTestBase.cs:25-31`, Crossplane throughout.
- Sync-over-async in infra: `TestApp.cs:129-130`, `TestCluster.cs:347`, `KubernetesTestRuntimeServiceCollectionExtensions.cs:61`.
- `TestWait.UntilValueAsync` returns `null` on timeout instead of throwing (`TestWait.cs:109-111`).
- Reflection into production privates: `ClusterAuthorizationTests.cs:553-558` (`InvalidateSeededResource`), `ResourceYamlViewModelTests.cs` ×10, `ResourceEventsViewTests.cs` ×3, `ClusterViewTests.cs:69-74`.
- Disposal gaps: `NavigationViewModelTests` ~40/66 undisposed; `ResourceYamlViewModelTests` 53/103; `ResourcePropertiesViewThreadingTests.cs:30-41` none; windows without `try/finally` in `NavigationViewTests.cs:134-147`, `ClusterViewTests.cs:23,53`, `ThemedSvgImageTests.cs:69-82`, `HostWindowStyleTests.cs:25,39`.

## Low findings

- Empty files `GlobalUsings.GenericKubernetesObject.cs` (both copies, 0 bytes); `ClusterCrdModelCatalogTests.cs` class name mismatches filename.
- Temp kubeconfig GUID paths never deleted (`ClusterManagerTests.cs:17,42,87,133`) while rest of file uses `TemporaryDirectory`.
- `null!` in settings double (`ClusterManagerTests.cs:529`).
- Tautologies: `NavigationViewModelTests.cs:1086-1088`, `NavigationSectionTests.cs:239-240`; post-wait re-checks `ClusterRuntimeAssertions.cs:372,399`.
- Mixed xUnit `Assert.*` with Shouldly: Crossplane, `DesignTimePreviewTests.cs:11`, `ResourceListView*Tests`.
- Hardcoded UI strings instead of resources: `MainViewModelTests.cs:265`, `UpdateAvailableViewModelTests.cs:188`.
- `new AksClusterService(null!)` (`AksClusterServiceTests.cs:39`).
- Unused/duplicated `using` in 5 Cilium files; duplicates in `NavigationTestResourceConfigs.cs:7-14`.
- `Mock.Of<IHubbleConnectionFactory>()` never invoked (`FlowListViewModelTests.cs:23,52,171,220`).
- `HelmServiceTests.cs:38` reference-equality assert near-tautological.
- `LiveHelmVisualizationTests.cs:100-106` — `Graph!.Resources.Count` in catch can NRE; use `Graph?`.
- Pointless `[Obsolete]` on 6 documentation recording methods; `[Obsolete]` screenshot helper test `ServiceMapViewTests.cs:723-758`.
- `FakeKubernetesHttpApi` hygiene: `_requestUris` unbounded (`:185,368`), dead watch channels never removed (`:2898`), no-op updates bump `resourceVersion` + emit MODIFIED.

## Coverage gaps

- **KubeUI.Kubernetes**: `KubernetesOpenApiSchemaLoader.cs` (353 lines, trim-critical, no focused tests); `DefaultKubeConfigPathProvider`, `ClusterManagerStartupService`, `ClusterComparer`; pod-log multi-scope error branch (`PodLogSessionResolver.cs:23-46`); `ClusterManager.cs` failure/merge-conflict paths; typed `V2beta1APIGroupDiscoveryList` deserialization.
- **KubeUI.Controls**: `TopLevelAccessor`, `IUiRefreshClock`/`AvaloniaUiRefreshClock`, `ApplicationBrushResources`, `IViewModelViewFactory`, converters `NotEmptyCollectionConverter`/`StaticResourceConverter`/`StringEqualsConverter`/`StringNotNullOrEmptyConverter`/`StringNullOrWhiteSpaceConverter` — zero tests anywhere.
- **KubeUI.App**: `DockFactory` (0 dedicated tests), `Fluent.cs` (spot-checks only), `SettingsView.cs` (interaction-level only), `MyDialogManager`, `LogSink`, `AvaloniaPlatformServices`, `Utilities`, `AboutViewModel`/`HomeViewModel` incidental.
- **KubeUI.Features.Resources**: port-forwarding UI (`PortForwarderListView`/`PortForwarderListViewModel`) zero tests; pod metric cells; `YamlFoldingBehavior`/`YamlSyntaxHighlightingBehavior`; `PodConsoleConnectionBehavior`; Secret converters indirect only. **Docking layout persistence round-trip: no integration test anywhere** (root AGENTS §6 requires it).
- **KubeUI.Features.AI**: `Acp\Agent.cs`, `AgentSession`, `TerminalHandler`, `FileSystemHandler`, `ExecutableLocator`, `DenyByDefaultAgentPermissionService` — zero tests despite explicit AGENTS requirements.
- **KubeUI.Features.Azure**: 4 tests for 209-line service; discovery/credential-selection/import paths untested.
- **KubeUI.Features.Cilium**: `HubbleConnection` lifecycle (3 arg-validation tests only); `ServiceMapViewModel` (~600 lines untested); `IsCiliumInstalled` missing dot-boundary negative (`xcilium.io`).
- **KubeUI.Features.Crossplane**: VM 50k row cap / `RowLimitReached` untested; bounded-channel backpressure untested; no DI registration test.
- **KubeUI.Features.GatewayApi**: only `Matches()` negative group/kind cases missing.
- **KubeUI.Features.Helm**: only `HelmValuesEditorBehavior` untested (indirect via view).
- **Documentation.Tests**: recorder regressions invisible unless `KUBEUI_DOCS_VIDEO_DIR` set (by design).

## Rule compliance scorecard

| Rule | Kube | Avalonia | Clusters | Resources | Helm | Cilium | Crossplane | Gateway | AI | Azure |
|---|---|---|---|---|---|---|---|---|---|---|
| No sleeps / sync-over-async | Y | Y | Y | Y | Y | Y | Y | Y | Y | Y |
| No IKubernetes/runtime doubles | Y* | N | Y | Y | Y | N | N | Y | Y | Y |
| CT everywhere | Y | ~ | ~ | ~ | ~ | N | N | Y | ~ | Y |
| Backend matrix where needed | Y | ~ | Y | Y | n/a | N | N | n/a | n/a | n/a |
| Dispose owned objects | Y | ~ | N | ~ | ~ | Y | ~ | Y | ~ | Y |
| Test-owned workspace for config parse | n/a | Y | Y | N | n/a | n/a | n/a | Y | n/a | n/a |

*Moq in PortForwarderTests only. "~" = partial/inconsistent.

## Action plan (priority order)

1. **Delete** `tests\KubeUI.Kubernetes.Tests\PortForwarderTests.cs` + `ClusterScenarioAssertions.cs` (root). ~1,800 dead/duplicated lines; duplicate Kind-matrix run gone. Mechanical, low risk.
2. **Port McpToolsTests pod-log tests and all Crossplane tests off Moq** onto `TestClusterGenerator` + transport fake. Then drop Moq from `KubeUI.Testing.csproj` (or scope it to projects that need it).
3. **Dispose `TestCluster` in `CreateClusterAsync`** (or return it) — fixes informer leak + `ClusterManager` accumulation for every feature suite at once.
4. **Fix fake transport contract**: atomic create (409), lowercase-name validation, non-v1 policy-resource versions, fake-path generator cleanup, admin-client condition-handler parity.
5. **Add config snapshot/reset to `CreateClusterAsync`**; clear `TestApp.Notification` in setup.
6. **Rewrite timing asserts** (`< 150ms` stopwatch, `Observable.Timer(100ms)`, instant negative asserts) as bounded predicate waits.
7. **Dispose pass**: uniform `using var`/`await using` in NavigationViewModelTests, ResourceYamlViewModelTests, ClusterWorkspaceTests; `try/finally` for windows.
8. **Add missing matrices** for Crossplane diff detection and Cilium cluster-extension logic.
9. **Fill high-value gaps**: `KubernetesOpenApiSchemaLoader`, ACP `Agent`/`AgentSession`/`TerminalHandler`/`FileSystemHandler`, AKS discovery paths, docking layout persistence round-trip, port-forwarder list VM, `ServiceMapViewModel` pipeline.
10. Hygiene sweep: empty files, mixed assert styles, hardcoded strings, temp-file litter, `[Obsolete]` misuse.

Fixes 1-3 mechanical, low risk. 4-6 remove real flake/fake-divergence risk. 7-10 quality debt, not rot.
