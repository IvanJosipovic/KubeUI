# Shared Controls Extraction Plan

**Goal:** Minimize direct Avalonia control usage in features by promoting recurring UI patterns to `KubeUI.Controls`.

---

## Pattern Analysis Across Features

| Pattern | Resources | Helm | AI | Clusters | Cilium | **Extract?** |
|---------|-----------|------|-----|----------|--------|--------------|
| **Toolbar with icon buttons + item count** | ✅ (PodLogsView, ResourceList) | ✅ (Release/Repo lists) | ❌ | ✅ (ClusterListView) | ✅ (FlowListView) | **YES** |
| **Search/filter toolbar with combo/tree selector** | ✅ (PodLogsView TreeComboBox) | ✅ (Chart/Repo combo) | ❌ | ❌ | ✅ (Flow namespace selector) | **YES** |
| **Namespace selector with anchor lock** | ✅ (VisualizationView, ResourceListView) | ✅ (HelmReleasesView) | ❌ | ❌ | ✅ (ServiceMapView, FlowListView) | **YES** |
| **Properties panel (key/value rows)** | ✅ (PropertyItem, ExpandableSection) | ❌ | ❌ | ❌ | ❌ | **YES** |
| **YAML/Text editor with validation + folding** | ✅ (PodLogsView, ResourceYamlView) | ✅ (HelmReleaseEditorView) | ❌ | ❌ | ❌ | **YES** |
| **Data grid with context menu + selection** | ✅ (DynamicTableView everywhere) | ✅ (DynamicTableView) | ❌ | ✅ (DynamicTableView) | ✅ (DynamicTableView) | Already shared |
| **Empty state / placeholder view** | ✅ (multiple) | ✅ (Helm) | ❌ | ❌ | ✅ (Cilium) | **YES** |
| **Loading overlay / skeleton** | ✅ (DataDisplay) | ✅ (Helm editors) | ❌ | ❌ | ❌ | **YES** |
| **Validation error banner** | ✅ (DataDisplay FAInfoBar) | ✅ (HelmReleaseEditorView) | ❌ | ❌ | ❌ | **YES** |
| **Confirm dialog / flyout patterns** | ✅ (multiple) | ✅ (Helm) | ❌ | ✅ (Cluster delete) | ❌ | **YES** |
| **Scroll-to-bottom / follow toggle** | ✅ (PodLogsView) | ❌ | ✅ (AgentChatView) | ❌ | ❌ | **YES** |
| **Tree-based resource selector** | ✅ (PodLogsView TreeComboBox) | ❌ | ❌ | ❌ | ❌ | **YES** |
| **Icon + text action buttons (toolbars)** | ✅ (everywhere) | ✅ (everywhere) | ✅ (AI) | ✅ (Clusters) | ✅ (Cilium) | **YES** |
| **Status badge/chip (connected/error/warning)** | ✅ (PodLogsView) | ❌ | ❌ | ❌ | ✅ (FlowListView) | **YES** |
| **Split view (sidebar + detail)** | ❌ | ❌ | ❌ | ❌ | ❌ | Not yet |

---

## Recommended Controls for KubeUI.Controls

### 1. `KubeToolBar` — Standardized toolbar pattern
```csharp
// Current duplication: ClusterListView, PodLogsView, FlowListView, Helm lists
public sealed class KubeToolBar : UserControl
{
    // Left: primary actions (Add, Clear, Refresh)
    // Center: item count / status text
    // Right: filter selectors, search, view options
    // Built-in: min height 32, spacing 2, margin 2, stretch spacer
}
```
**Consumers:** All feature list views

---

### 2. `KubePropertyGrid` — Key/value property display
```csharp
// Current: PropertyItem, ExpandableSection in Resources
// Also needed by: Cilium ServiceMapNodePropertiesView
public sealed class KubePropertyGrid : Control
{
    // Rows: Key (SelectableTextBlock) + Value (ScrollViewer + SelectableTextBlock)
    // Sections: ExpandableSection with styled Expander
    // Read-only + Edit modes
    // Auto-height rows, max height 200 for values
}
```
**Consumers:** ResourcePropertiesView, ServiceMapNodePropertiesView, future feature property panels

---

### 3. `KubeYamlEditor` — Standardized YAML editing
```csharp
// Current: PodLogsView TextEditor, HelmReleaseEditorView TextEditor, ResourceYamlView
public sealed class KubeYamlEditor : UserControl
{
    // TextEditor with:
    // - CodeFontFamilyResourceKey / CodeFontSizeResourceKey
    // - Line numbers, word wrap toggle
    // - YamlSyntaxHighlightingBehavior
    // - YamlFoldingBehavior
    // - Validation error binding (DataValidationErrors)
    // - Read-only mode
    // - Context menu (Copy, Select All)
    // - Min/Max height bounds
}
```
**Consumers:** ResourceYamlView, HelmReleaseEditorView, PodLogsView, future YAML editors

---

### 4. `KubeValidationBanner` — Inline validation/error display
```csharp
// Current: DataDisplay FAInfoBar, HelmReleaseEditorView (implicit)
// Uses FluentAvalonia FAInfoBar internally
public sealed class KubeValidationBanner : UserControl
{
    // Title, Message, Severity (Info/Warning/Error/Success)
    // Dismissible, bindable IsOpen
    // Auto-hide after timeout (optional)
}
```
**Consumers:** DataDisplay, Helm editors, any form validation

---

### 5. `KubeEmptyState` — Consistent empty/placeholder view
```csharp
// Current: Ad-hoc in each feature
public sealed class KubeEmptyState : UserControl
{
    // Icon (FluentIcon), Title, Description, Action button (optional)
    // Standardized spacing, alignment, theming
}
```
**Consumers:** All feature lists when no data

---

### 6. `KubeLoadingOverlay` — Loading skeleton/placeholder
```csharp
// Current: DataDisplay edit mode, Helm editors loading charts
public sealed class KubeLoadingOverlay : UserControl
{
    // Overlay with spinner/skeleton
    // Bindable IsLoading
    // Optional progress text
}
```
**Consumers:** DataDisplay, Helm editors, any async-loaded view

---

### 7. `KubeTreeComboBox` — Hierarchical selector (Pod logs pattern)
```csharp
// Current: PodLogsView TreeComboBox for Resource->Pod->Container
// Also needed by: Future namespace/cluster selectors
public sealed class KubeTreeComboBox : UserControl
{
    // TreeComboBox with:
    // - CheckBox items with two-way IsChecked
    // - Auto-expand parents
    // - Partial selection state
    // - Max dropdown height 500, max height 32
    // - Placeholder text
}
```
**Consumers:** PodLogsView, future multi-level selectors

---

### 8. `KubeNamespaceSelector` — Namespace picker with anchor lock
```csharp
// Current: VisualizationView, ServiceMapView, FlowListView, HelmReleasesView
// Pattern: MultiComboBox/ComboBox/TreeComboBox + Link ToggleButton
public sealed class KubeNamespaceSelector : UserControl
{
    // Selection modes
    public NamespaceSelectionMode Mode { get; set; }  // Single | Multi | Tree (hierarchical)
    
    // Data
    public IEnumerable<V1Namespace> Namespaces { get; set; }
    public IList<V1Namespace> SelectedNamespaces { get; set; }
    
    // Anchor lock (link to cluster's active namespace)
    public bool IsLinkedToClusterSelection { get; set; }
    public bool IsLinkEnabled { get; set; } = true;
    public ICommand? ToggleLinkCommand { get; set; }
    
    // UI
    public string PlaceholderText { get; set; } = "Select namespace";
    public bool ShowClearButton { get; set; } = true;
    public double Width { get; set; } = 200;
    public double MinWidth { get; set; } = 140;
    public double MaxDropDownHeight { get; set; } = 500;
    
    // Internal: Uses ComboBox (Single), Ursa.MultiComboBox (Multi), Ursa.TreeComboBox (Tree)
    // Link button: KubeIconButton with Icon.Link, tooltip "Link to cluster namespace"
}
```

**Mode Mapping:**
| Mode | Internal Control | Used By |
|------|-----------------|---------|
| `Single` | `ComboBox` | Helm release list |
| `Multi` | `Ursa.MultiComboBox` | VisualizationView, ServiceMapView, ResourceListView |
| `Tree` | `Ursa.TreeComboBox` | FlowListView (Resource→Pod→Container) |

**Consumers:** VisualizationView, ResourceListView, HelmReleasesView, ServiceMapView, FlowListView

---

### 10. `KubeFollowToggle` — Auto-scroll/follow control
```csharp
// Current: PodLogsView Follow Logs toggle, AgentChatView auto-scroll
public sealed class KubeFollowToggle : ToggleButton
{
    // Icon: ArrowDownload (follow) / ArrowUp (paused)
    // Tooltip: "Follow Logs" / "Follow Conversation"
    // Logic: viewport/extent tracking, half-line threshold
    // Shows pending count when paused
}
```
**Consumers:** PodLogsView, AgentChatView, any streaming log/chat view

---

### 11. `KubeStatusBadge` — Connection/status indicator
```csharp
// Current: PodLogsView connection error/warning, FlowListView connection status
public sealed class KubeStatusBadge : UserControl
{
    // States: Connected, Disconnected, Error, Warning, Loading
    // Icon + text, color-coded by severity
    // Compact inline or standalone
}
```
**Consumers:** PodLogsView, FlowListView, any connection-aware view

---

### 12. `KubeActionButton` / `KubeIconButton` — Standardized icon buttons
```csharp
// Current: Ad-hoc Button + FluentIcon + ToolTip everywhere
public sealed class KubeActionButton : Button
{
    // Icon (FluentIcon), ToolTip, Command
    // Standard sizes: 32x32 (toolbar), 24x24 (inline)
    // Standard padding: 4
    // Disabled/visible binding helpers
}

public sealed class KubeIconButton : Button
{
    // Icon-only, no text
    // Sizes: 16, 20, 24, 32
}
```
**Consumers:** Every toolbar, action bar, context menu

---

## Migration Strategy

### Phase 1: Foundation Controls (Week 1)
| Control | Source to Extract From | Tests |
|---------|------------------------|-------|
| `KubeActionButton` / `KubeIconButton` | All features (mechanical) | Unit |
| `KubeStatusBadge` | PodLogsView, FlowListView | Unit |
| `KubeEmptyState` | Resources, Helm, Cilium | Unit |

### Phase 2: Composite Controls (Week 2)
| Control | Source to Extract From | Tests |
|---------|------------------------|-------|
| `KubeToolBar` | ClusterListView, PodLogsView, FlowListView | UI |
| `KubePropertyGrid` | PropertyItem, ExpandableSection | UI |
| `KubeValidationBanner` | DataDisplay, HelmReleaseEditorView | UI |
| `KubeNamespaceSelector` | VisualizationView, ServiceMapView, FlowListView | UI |

### Phase 3: Complex Editors (Week 3)
| Control | Source to Extract From | Tests |
|---------|------------------------|-------|
| `KubeYamlEditor` | PodLogsView, HelmReleaseEditorView, ResourceYamlView | UI + Integration |
| `KubeTreeComboBox` | PodLogsView | UI |
| `KubeFollowToggle` | PodLogsView, AgentChatView | UI |
| `KubeLoadingOverlay` | DataDisplay, Helm editors | Unit |

---

## Breaking Changes for Features

After extraction, features **must** use shared controls:

```csharp
// BEFORE (feature code)
new Button()
    .Command(vm.ClearCommand)
    .ToolTip_Tip(Assets.Resources.Clear)
    .Content(new FluentIcon().Icon(Icon.Broom))

// AFTER (feature code)
new KubeActionButton()
    .Command(vm.ClearCommand)
    .Icon(Icon.Broom)
    .ToolTip(Assets.Resources.Clear)
```

```csharp
// BEFORE
new TextEditor()
    .FontFamily(new DynamicResourceExtension(Typography.CodeFontFamilyResourceKey))
    .FontSize(new DynamicResourceExtension(Typography.CodeFontSizeResourceKey))
    .ShowLineNumbers(true)
    .WordWrap(vm, x => x.WordWrap)
    .Options(new TextEditorOptions { ... })
    .BindValue(DataValidationErrors.ErrorsProperty, ...)

// AFTER
new KubeYamlEditor()
    .Document(vm, x => x.Logs)
    .ReadOnly(true)
    .WordWrap(vm, x => x.WordWrap)
    .ValidationErrors(vm, x => x.ValidationErrors)
```

### Namespace Selector Migration

```csharp
// BEFORE (VisualizationView)
new Ursa.Controls.MultiComboBox()
    .Width(200)
    .MinWidth(140)
    .MaxHeight(20)
    .Classes("ClearButton")
    .ItemsSource(vm, x => x.Cluster.Runtime.Namespaces)
    .SelectedItems(vm, x => x.SelectedNamespaces)
    .PlaceholderText(Assets.Resources.ResourceListView_SelectNamespace)
    .SelectedItemTemplate(template)
    .ItemTemplate(template)
    .IsEnabled(vm, x => x.IsNamespaceSelectionEnabled)
    .IsVisible(vm, x => x.IsNamespaceSelectorVisible),

// Link button (separate)
new ToggleButton()
    .Margin(0, 0, 2, 0)
    .IsChecked(vm, x => x.IsNamespaceSelectionLinked, BindingMode.TwoWay)
    .ToolTip_Tip(Assets.Resources.ResourceListView_NamespaceLink)
    .Content(new FluentIcon().Icon(Icon.Link))
    .IsEnabled(vm, x => x.IsNamespaceSelectionEnabled)
    .IsVisible(vm, x => x.IsNamespaceSelectorVisible)

// AFTER
new KubeNamespaceSelector()
    .Mode(NamespaceSelectionMode.Multi)
    .Namespaces(vm, x => x.Cluster.Runtime.Namespaces)
    .SelectedNamespaces(vm, x => x.SelectedNamespaces)
    .IsLinkedToClusterSelection(vm, x => x.IsNamespaceSelectionLinked)
    .PlaceholderText(Assets.Resources.ResourceListView_SelectNamespace)
    .IsEnabled(vm, x => x.IsNamespaceSelectionEnabled)
    .IsVisible(vm, x => x.IsNamespaceSelectorVisible)
```

---

## Validation Checklist

- [ ] Each control has unit tests in `tests/KubeUI.Avalonia.Tests/Controls/`
- [ ] Each control has design-time preview support
- [ ] Controls use `DynamicResource` for theming (colors, fonts, spacing)
- [ ] Controls follow compiled-binding patterns
- [ ] Controls documented with XML comments
- [ ] Migration doesn't change visual behavior (pixel-perfect)
- [ ] All features updated to use shared controls
- [ ] Trim analysis passes (no new warnings)

---

## Related

- `docs/architecture/restructuring-plan.md` — Overall restructuring
- `src/KubeUI.Controls/AGENTS.md` — Controls project guide
- `src/KubeUI.App/AGENTS.md` — App UI patterns