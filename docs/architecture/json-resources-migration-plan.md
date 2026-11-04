# JSON resources migration plan

**Status:** Implemented (Phases 0–3; Phases 4–5 deferred)
**Decisions:** Migrate call sites to Lingua observables; defer the POC hot-reload handler; invariant culture only for now.
**Progress:** All eight resource-owning projects are on `Assets/Resources.json` behind per-project Lingua managers. No `.resx` remains under `src/`; the JSON contract test enforces flat-string files, key naming, and the no-resx invariant. `KubeUI.Avalonia` was renamed to `KubeUI.Foundation` while scaffolding, to remove the namespace collision with the global `Avalonia` namespace that broke XML doc crefs in every `KubeUI.*` namespace.

## As-built notes (deviations from the original plan)

- Manager aliases: `L10n` for the owning feature, `SharedL10n` for the public foundation manager, `ResourceL10n` for the Resources feature. Test projects declare manager aliases in their own csproj (not `tests/FeatureTests.props`) because namespace aliases there leaked `internal` access between unrelated test projects.
- Cross-feature and App access to internal feature managers uses `InternalsVisibleTo` (App, Cilium, Crossplane, Helm ← Resources; App ← Clusters; test projects as before).
- The current-value helper is `KubeUI.Foundation.Localization.LinguaObservableExtensions.CurrentValue()`.
- Live control properties bind via `ToBinding()`; object-initializer control properties and structural strings (column keys, navigation names, resource-config metadata, dialog strings) read `CurrentValue()` and form the Phase 4 backlog for the future culture-switch work.

## Goal

Replace the eight `Assets/Resources.resx` files and the MSBuild-generated `obj/Resources.Designer.cs` strongly typed classes with JSON resource files consumed by [Irihi.Lingua](../../poc/Irihi.Lingua.HotReload.Minimal/README.md). Lingua turns each key into an `IObservable<string?>` on a per-project manager, so UI can update when a value or culture changes.

The key names, values, and the `{ViewOrControlName}_{Name}` / `{ResourceConfigName}_{Name}` / `Shared_{Name}` naming convention stay exactly as they are today. Only the storage format and the access API change.

## Non-goals

- No second culture file and no culture switcher in this change (decision deferred).
- No `.NET hot reload` handler in this change (decision deferred).
- No redesign of table columns, navigation models, or resource-config metadata into reactive structures. Those are captured as plain strings today and stay plain strings; see [Deferred work](#deferred-work).

## Current state

| Project | `Resources.resx` entries | Source call sites | Test call sites | Notes |
|---|---:|---:|---:|---|
| `KubeUI.Foundation` | 201 | 0 | 53 | Consumed from other projects as `SharedAssets.Resources.*` (283 sites) and as `Assets.Resources.*` from `KubeUI.App` |
| `KubeUI.App` | — | 146 | — | `Assets` alias points at `KubeUI.Foundation.Assets` |
| `KubeUI.Features.Resources` | 444 | 609 | 70 | Also consumes `SharedAssets` (116 sites) |
| `KubeUI.Features.Helm` | 96 | 118 | 20 | Also consumes `SharedAssets` (12 sites) |
| `KubeUI.Features.Cilium` | 92 | 101 | 23 | |
| `KubeUI.Features.Clusters` | 47 | 49 | 3 | |
| `KubeUI.Features.AI` | 33 | 29 | — | |
| `KubeUI.Features.Crossplane` | 22 | 25 | 4 | Also consumes `ResourceAssets` |
| `KubeUI.Features.GatewayApi` | 2 | 3 | — | Smallest surface; pilot project |

Total: 937 keys, roughly 1,250 call sites. All UI is code-based Avalonia (`Avalonia.Markup.Declarative`); there are no `.axaml` files. Today every call site is a plain synchronous `string` read. There is no culture switching anywhere in the app.

Each project currently has this csproj shape:

```xml
<Compile Include="$(BaseIntermediateOutputPath)\Resources.Designer.cs"
         Condition="'$(DesignTimeBuild)' == 'true'" AutoGen="true" DesignTime="true" Visible="false" />
<EmbeddedResource Update="Assets\Resources.resx">
  <PublicClass>true</PublicClass>
  <StronglyTypedLanguage>CSharp</StronglyTypedLanguage>
  <StronglyTypedNamespace>KubeUI.Features.X.Assets</StronglyTypedNamespace>
  <StronglyTypedClassName>Resources</StronglyTypedClassName>
  <StronglyTypedFileName>$(BaseIntermediateOutputPath)\Resources.Designer.cs</StronglyTypedFileName>
</EmbeddedResource>
```

## Target architecture

### Files

```text
src/KubeUI.Features.X/
├── Assets/
│   ├── Resources.json        # base / invariant resource values (flat keys)
│   └── LanguageManager.cs    # [LinguaManager("./Resources.json")] partial class
└── KubeUI.Features.X.csproj
```

`Resources.json` is compile-time input only (`AdditionalFiles`); it is never embedded as an `AvaloniaResource` and never read from disk at runtime. Lingua emits the values as string literals into generated code, so trimming and NativeAOT stay safe.

`Resources.json` format rules:

- One flat object; no nested objects (nested keys would be flattened with `_`, which can collide with real keys that already contain `_`).
- Every value is a `string`; no numbers, booleans, or `null`.
- Keys are byte-for-byte identical to today's resx `data/@name` values.
- Values are byte-for-byte identical, including leading/trailing whitespace (`AgentChatView_ToolInputFormat` is `" {0}"`), quotes, backslashes, and newlines.
- UTF-8, 2-space indent, one key per line, trailing newline.

Example:

```json
{
  "MRDiffDetectionView_Title": "MR Diff Detection",
  "MRDiffDetectionView_SelectProvider": "Select a provider",
  "MRDiffDetectionView_Connecting": "Connecting to {0}"
}
```

### Manager declaration

One manager per resource-owning project. The path is resolved relative to the `.cs` file that declares the manager, so keeping both files in `Assets/` is unambiguous.

```csharp
using Irihi.Lingua;

namespace KubeUI.Features.Crossplane.Assets;

/// <summary>Localized strings for the Crossplane feature.</summary>
[LinguaManager("./Resources.json")]
internal sealed partial class LanguageManager;
```

`KubeUI.Foundation.Assets.LanguageManager` must be `public` because five feature assemblies and `KubeUI.App` consume the shared strings. Feature managers stay `internal`; their test projects already have `InternalsVisibleTo` (add it for `KubeUI.Features.Clusters.Tests` if that project needs the manager).

Lingua generates on each manager:

- `Instance` (singleton), `Keys` (one `LinguaKey` per resource), `CultureChanges`, `UpdateCulture`, `GetObservable`, `AddResources`, `ClearRuntimeResources`, `GetTranslations`.
- One `IObservable<string?>` property per key, named exactly like the resx key.

### csproj changes

Replace the `Compile`/`EmbeddedResource` block with:

```xml
<ItemGroup>
  <AdditionalFiles Include="Assets\Resources*.json" />
  <Watch Include="Assets\Resources*.json" />
  <PackageReference Include="Irihi.Lingua" />
</ItemGroup>
```

`Assets\Resources*.json` covers future `Resources.<culture>.json` files. `Watch` is harmless now and becomes useful when the hot-reload handler lands.

In `KubeUI.Foundation` only, exclude the JSON from the `AvaloniaResource` glob:

```xml
<AvaloniaResource Include="Assets\**" Exclude="Assets\Resources*.json" />
```

`Directory.Packages.props` already pins `Irihi.Lingua` at `1.1.0`; no version change is needed.

### Aliases

Each project gets a short global alias for the managers it consumes. Existing namespace aliases (`Assets`, `SharedAssets`, `ResourceAssets`) keep working for `LanguageManager` discovery, but short aliases keep call sites readable.

```xml
<!-- In the project that owns the manager -->
<Using Include="KubeUI.Features.Crossplane.Assets.LanguageManager" Alias="L10n" />

<!-- In projects that consume shared strings (all features, App, tests) -->
<Using Include="KubeUI.Foundation.Assets.LanguageManager" Alias="SharedL10n" />

<!-- Where a feature consumes another feature's strings -->
<Using Include="KubeUI.Features.Resources.Assets.LanguageManager" Alias="ResourceL10n" />
```

### Shared helper

Lingua's generated property is typed `IObservable<string?>`, but the concrete value lives on `LinguaObservableString.CurrentValue`; there is no public value accessor on the interface. Add one documented, tested helper in `KubeUI.Foundation`:

```csharp
namespace KubeUI.Foundation.Localization;

/// <summary>Value access for Lingua resource observables.</summary>
public static class LinguaObservableExtensions
{
    /// <summary>
    /// Returns the value currently published by a Lingua resource observable
    /// without creating a subscription. Use only where an observable cannot be bound
    /// (structural strings, one-shot messages, comparisons, URIs).
    /// </summary>
    public static string? CurrentValue(this IObservable<string?> source)
        => ((LinguaObservableString)source).CurrentValue;
}
```

This cast is safe because Lingua's generator always backs the property with a `LinguaObservableString`. It is the only place in the app that performs it.

Add `<Using Include="KubeUI.Foundation.Localization" />` to the consuming projects.

Binding needs no helper: `Avalonia.AvaloniaObjectExtensions.ToBinding()` converts any `IObservable<T>` to a `BindingBase`, and the `Avalonia.Markup.Declarative` fluent extensions already expose `BindingBase` overloads (`Text`, `Header`, `Content`, `PlaceholderText`, `ToolTip_Tip`, `Watermark`, …).

## Call-site migration rules

Counts below are from the current tree and overlap (a line can be in two categories).

| Context | Count | Target pattern |
|---|---:|---|
| Fluent control property (`.Text`, `.Header`, `.Content`, `.Title`, `.PlaceholderText`, `.ToolTip_Tip`, `.Watermark`) | 129 | `.Text(L10n.Instance.Key.ToBinding())` |
| Object initializer (`new TextBlock { Text = … }`) | 110 | `new TextBlock { … }.Text(L10n.Instance.Key.ToBinding())` |
| Tooltip attached property (`ToolTip.SetTip`) | 47 | `control.ToolTip_Tip(L10n.Instance.Key.ToBinding())` |
| Column keys / navigation names / resource-config metadata | ~250 | `L10n.Instance.Key.CurrentValue()!` |
| One-shot status, log, exception, notification text | ~49 | `… .CurrentValue()!` |
| `string.Format` for live text | 43 | `L10n.Keys.Key.CreateFormat().Arg(…).Build().ToBinding()` |
| `string.Format` for one-shot messages | (subset of 43) | `string.Format(CultureInfo.CurrentCulture, L10n.Instance.Key.CurrentValue()!, args)` |
| Comparisons / `Equals` | ~70 (mostly tests) | `.CurrentValue()` |
| `new Uri(…)` | 2 | `.CurrentValue()!` |
| Tests | 173 | `.CurrentValue()` |

### Rule 1 — bind, don't read

Any value that is a property of a live control binds to the observable:

```csharp
// Before
new TextBlock { Text = Assets.Resources.MRDiffDetectionView_Title };

// After
new TextBlock().Text(L10n.Instance.MRDiffDetectionView_Title.ToBinding());
```

```csharp
// Before
button.ToolTip_Tip(Assets.Resources.DataDisplay_Edit);

// After
button.ToolTip_Tip(L10n.Instance.DataDisplay_Edit.ToBinding());
```

```csharp
// Before
Header = Assets.Resources.Shared_Status

// After (object initializer, then fluent)
new Border { /* … */ }.Header(SharedL10n.Instance.Shared_Status.ToBinding());
```

Bindings created through the fluent `BindingBase` overloads are owned by the property system and are cleaned up with the control; do not hand out subscriptions.

### Rule 2 — `CurrentValue()` only where binding is impossible

Structural strings are read once when a model is built and are not bindable without rebuilding that model:

```csharp
// Column headers
CreateColumn("name", L10n.Instance.MRDiffDetectionView_Name.CurrentValue()!, typeof(string), static row => row.Name);

// Navigation section name
new NavigationSection(…, SharedL10n.Instance.NavigationView_Crossplane.CurrentValue()!, …);

// Resource-config metadata
Title = L10n.Instance.ResourcePropertiesView_Title.CurrentValue()!;
```

One-shot messages, logs, exceptions, and URIs use the same pattern:

```csharp
SetStatus(L10n.Instance.MRDiffDetectionView_SelectProvider.CurrentValue()!, generation);
ResourceUtilities.HandleException(_logger, _notificationManager, ex,
    L10n.Instance.DeploymentRolloutHistoryViewModel_LoadError.CurrentValue()!, sendNotification: true);
.NavigateUri(new Uri(SharedL10n.Instance.AboutView_ContactEmailUri.CurrentValue()!))
```

Every `CurrentValue()` call site is a known non-reactive site. A grep for `CurrentValue()` becomes the audit list for the future culture-change refresh work.

### Rule 3 — format strings

Displayed format strings become Lingua format observables so the text and its arguments update together:

```csharp
// Before
Text = string.Format(CultureInfo.CurrentCulture, Assets.Resources.ScalableWorkloadConfig_BulkPrompt!, selected.Count),

// After
Text = L10n.Keys.ScalableWorkloadConfig_BulkPrompt.CreateFormat()
    .Arg(selected.Count)
    .Build()
    .ToBinding(),
```

For values that change over time, prefer a pull getter so `Refresh()` can re-read them:

```csharp
L10n.Keys.ResourceListView_Delete_Content.CreateFormat()
    .Arg(() => items.Count)
    .Build()
```

One-shot format strings (exceptions, notifications) keep `string.Format` but read the template via `CurrentValue()`:

```csharp
ResourceUtilities.HandleException(_logger, _notificationManager, ex,
    string.Format(CultureInfo.CurrentCulture,
        L10n.Instance.ScalableWorkloadConfig_ScaleError.CurrentValue()!,
        workload.Namespace(), workload.Name()),
    sendNotification: true);
```

Lingua formats with the manager's `CurrentCulture`; `string.Format` uses `CultureInfo.CurrentCulture`. With invariant-only resources today these are equivalent; revisit when cultures land.

### Rule 4 — view-model strings

A view-model property that is set once (title, status default, error text) reads `CurrentValue()`. A property that must react to culture changes is exposed as `IObservable<string?>` and bound in the view:

```csharp
// View-model
public IObservable<string?> Title => L10n.Instance.ResourcePropertiesView_Title;

// View
new TextBlock().Text(ViewModel.Title.ToBinding());
```

Do not subscribe to Lingua observables inside view models unless the view model already owns a disposable lifetime; the view binding path covers the normal case.

### Rule 5 — tests

Tests compare against the current value explicitly:

```csharp
items.Any(x => x.Key == ResourceL10n.Instance.ResourcePropertiesView_Name.CurrentValue()).ShouldBeTrue();
```

## Conversion procedure

1. Write a one-off `resx -> json` converter (Python is fine; do not commit it). For every `<data>` element, copy `name` and the `value` text verbatim into the JSON object, preserving order. Ignore the empty `<comment/>` elements.
2. Run a round-trip check: parse the resx and the JSON and assert identical key sets, identical values (including whitespace), no duplicates, and no non-string values.
3. Delete `Assets/Resources.resx` and run `dotnet clean` so stale `obj/Resources.Designer.cs` files cannot be compiled.
4. Apply the csproj, manager, alias, and call-site changes per project.
5. Build the project and run its tests before moving to the next project.

## Phasing

### Phase 0 — scaffolding (done)

- Added `LinguaObservableExtensions.CurrentValue()` in `KubeUI.Foundation/Localization` with four unit tests, including a test-project fixture manager (`TestLanguageManager`) that covers `AddResources` + `UpdateCulture` republish.
- Added the `Irihi.Lingua` package reference to the eight resource-owning projects.
- Manager aliases (`L10n`, `SharedL10n`, `ResourceL10n`) and the `KubeUI.Foundation.Localization` global using land per project at conversion time, because the managers do not exist before their project converts.
- The JSON contract test lands with the pilot (there are no JSON files to validate before the first conversion).
- Exit criteria met: solution builds, no resource files touched.

### Phase 1 — pilot: `KubeUI.Features.GatewayApi` (done)

2 keys, 3 call sites. Convert end-to-end: JSON, manager, csproj, call sites, tests, docs for the pattern. This proves the toolchain and the call-site rules on the smallest surface.

- Exit: project + tests build and pass; `Resources.resx` gone; no `Resources.Designer.cs` reference remains.

### Phase 2 — small and medium features (done)

Converted in order: `Crossplane`, `AI`, `Clusters`, `Cilium`, `Helm`, then `KubeUI.Foundation` with all shared consumers (`KubeUI.App` `Assets.Resources.*`, `Resources`/`Helm` `SharedAssets.Resources.*`) converted in the same step. Each project's test suite ran green before moving on.

### Phase 3 — `KubeUI.Features.Resources` (done)

444 keys, 609 source sites, 70 test sites, plus 116 shared sites. Convert in sub-batches by folder (Properties, ResourceConfigs/Core, Workloads, Network, Configuration, AccessControl, CustomResourceDefinition, Visualization) so each batch is reviewable.

- Exit criteria met: full solution build green; JSON contract test extended with the `no resx under src/` assertion; per-project wiring tests added for every manager.

### Phase 4 — reactive pass on live surfaces (deferred backlog)

Convert the remaining bindable-but-not-yet-bound sites on long-lived surfaces: shell/navigation, document headers, dialogs, toolbars, status bars. Use the `CurrentValue()` audit list as the backlog. Structural strings stay as-is until culture switching is implemented.

- Exit: all live shell chrome binds to observables; `CurrentValue()` remains only in structural/one-shot code.

### Phase 5 — deferred

- Hot-reload metadata handler (port `poc/Irihi.Lingua.HotReload.Minimal/LanguageManager.cs`): a small partial per manager that calls a shared reload algorithm; document that added/removed keys need a restart.
- Culture files (`Resources.<culture>.json`), `CulturePicker`/settings wiring, and a culture-change refresh path for structural strings.
- The `CulturePicker` theme include (`avares://Irihi.Lingua.Core/Themes/Generic.axaml`) is only needed when the picker is added.

## Testing and verification

Build and test commands follow the repository convention:

```sh
dotnet build --tl:off -clp:ErrorsOnly
dotnet test --no-build
```

Add these tests:

1. **JSON contract test** (repo-level, e.g. in `KubeUI.Avalonia.Tests`): locate `src/**/Assets/Resources*.json` via `[CallerFilePath]`, assert each file is a flat object of strings, keys are unique and match the `{ViewOrControlName}_{Name}` / `{ResourceConfigName}_{Name}` / `Shared_{Name}` conventions, values are non-empty, and no `src/**/*.resx` exists.
2. **Manager wiring test** per resource-owning project: one sample key per manager returns the expected JSON value through `Instance.<Key>.CurrentValue()`, and the observable emits that value on subscribe.
3. **Binding test** (headless Avalonia): bind a `TextBlock.Text` to a manager observable via `ToBinding()` and assert the text; call `UpdateCulture(CultureInfo.InvariantCulture)` and assert the value is republished without recreating the control.
4. **`CurrentValue()` helper test**: returns the current value and tracks `UpdateCulture` for a manager with an added runtime resource (`AddResources` + `UpdateCulture`).

Run the full suite after Phase 3. Release builds are the trimming gate: `dotnet build -c Release --tl:off -clp:ErrorsOnly` must stay clean for `KubeUI.Features.AI` and `KubeUI.Kubernetes`, and IL2091 must not appear for `KubeUI.Foundation`.

## Risks and mitigations

| Risk | Mitigation |
|---|---|
| `LinguaKey` implicit conversion returns the key name, not the value | Never use `Keys.X` as a value; use `Instance.X` + `CurrentValue()` or `Keys.X.CreateFormat()`. Enforced by review and the pilot. |
| Binding an observable to a control leaks the control | Use the `ToBinding()`/`BindingBase` path, which the property system owns; never keep a manual `Subscribe` without disposing it. |
| Structural strings do not react to culture changes | Explicit `CurrentValue()` + audit list; culture-change refresh is Phase 5. |
| Losing whitespace or escapes during conversion | Round-trip check before deleting each resx; `AgentChatView_ToolInputFormat` (`" {0}"`) is the canary. |
| Lingua flattens nested JSON and can collide with underscore keys | JSON contract test requires a flat object. |
| Stale `obj/Resources.Designer.cs` keeps compiling after resx deletion | `dotnet clean` and remove the `Compile Include` item in the same commit. |
| `KubeUI.Foundation`'s `AvaloniaResource Include="Assets\**"` embeds the JSON | Add the `Exclude="Assets\Resources*.json"` entry. |
| Public shared manager exposes Lingua types | Only `KubeUI.Foundation`'s manager is public; feature managers stay internal. |
| Package adds an Avalonia dependency to a non-UI project | All eight resource-owning projects already reference Avalonia. |
| Missing-key warnings when culture files are added later | Lingua already reports `LINGUA002`; treat it as a build warning to fix, not suppress. |

## Documentation updates

- Root `AGENTS.md` §7: replace “Resource keys in `Resources.resx`” with `Assets/Resources.json`, keep the naming convention, and state the binding rule (`ToBinding()` for live UI, `CurrentValue()` for structural/one-shot code).
- `src/KubeUI.Features.AI/AGENTS.md`: point at `Assets/Resources.json`.
- `src/KubeUI.Features.Helm/AGENTS.md`: point at `Assets/Resources.json`.
- Add a short `Assets/README.md` (or a section in the plan) explaining JSON shape, culture file naming, and that values are compile-time.

## Rollback

Each project is converted independently. Reverting a project means restoring its `Assets/Resources.resx`, csproj block, and call sites; no cross-project runtime state is introduced, and the JSON files are inert without the manager. `KubeUI.Foundation` and its shared consumers must be reverted together.
