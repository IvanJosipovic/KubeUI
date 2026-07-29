# Navigation Feature

## Current Behavior

- `NavigationViewModel` mirrors the cluster catalog. Adding, removing, replacing, or resetting catalog entries updates the corresponding cluster navigation nodes.
- A cluster navigation node is empty while disconnected. Its fixed cluster links are recreated when the runtime connection state changes; resource links are then populated from connected, initialized resource configurations.
- While a cluster remains connected, resource navigation is incremental:
  - resource configuration changes add, update, or remove only the affected resource link;
  - custom-resource-definition changes update the CRD branch and affected custom-resource links without rebuilding unrelated resource links;
  - cluster-name changes update navigation IDs in place.
- Resource links require `PermissionsLoaded` and the configuration's resolved `CanListAndWatch` value. Namespace-specific permission fallback is resolved by the workspace/configuration layer; this view model does not independently inspect namespace permissions.
- Navigation categories are reused by their generated IDs and empty generated categories are removed. Duplicate categories are not generally merged, so new code must not create duplicate category nodes.
- Port-forward navigation is recalculated only when the Pod resource configuration is processed. It is visible only when Pod list/watch permission and the port-forward create permission are available. The current implementation removes and recreates the port-forward link during that refresh.
- Resource count observables are attached only when both the matching navigation link and a seeded resource container exist. `ResourceSeeded` notifications are marshalled to the UI dispatcher; there is no retry loop for a missing link. A count stream preserves an initial zero and is retained when an existing resource link is updated.
- Connection failures update or open the shared cluster-error document. Missing namespace permission opens cluster settings and shows the permission prompt.
- Selecting a resource link opens its document; selecting a category toggles its expansion. Selecting a disconnected cluster starts connection asynchronously, and selecting an already connected cluster toggles expansion.

## Invariants

- Keep the navigation tree mutable and preserve existing resource-link instances during ordinary configuration updates whenever possible.
- Perform navigation collection mutations on the Avalonia UI thread, but do not handle resource-config events synchronously there. Resource-config events are batched off-thread, including any required configuration snapshot; the navigation update itself is then applied on the UI thread because it touches UI-owned collections and observable state.
- Do not add polling, unbounded dispatcher reposts, or full-tree rebuilds to solve ordering issues. Fix the owning lifecycle event or add a bounded, event-driven handoff.
- Preserve resource count observables and link identity when an ordinary resource configuration is updated. Structural changes such as connection transitions or required category moves may replace the relevant branch.

## Validation

- Preserve and extend the behaviors covered by `tests/KubeUI.Avalonia.Tests/Shell/Navigation/NavigationViewModelTests.cs` and `LimitedAccessNavigationTests.cs`.
- For navigation changes, add or update a focused Avalonia headless regression covering the exact lifecycle or permission transition, run it before broader validation, and verify both link structure and count behavior where applicable.
- At minimum, validate connected/disconnected transitions, incremental resource-config updates, CRD updates, Pod port-forward visibility, seeded/unseeded count behavior, and missing-namespace permission handling when those areas change.

## Use-case Coverage

- Catalog add, remove, replace, and reset: `cluster_catalog_changes_update_navigation_nodes`.
- Connection lifecycle and deferred population: `resource_navigation_items_populate_only_after_connect_completes`, `cluster_node_expands_after_successful_connect`, `cluster_context_menu_disconnect_clears_navigation_and_updates_menu`, and `resource_config_navigation_is_applied_after_background_processing`.
- Connection failures and namespace-permission prompts: `selecting_cluster_node_does_not_crash_when_connect_fails`, `selecting_cluster_node_opens_cluster_error_document_when_connect_fails`, `selecting_cluster_node_without_namespace_list_permission_opens_settings_and_prompt`, and `selecting_cluster_node_without_namespace_list_permission_reuses_existing_settings_document`.
- Namespace-scoped visibility: `selecting_cluster_node_with_namespace_fallback_shows_namespaced_resources_in_navigation`, `selecting_cluster_node_with_settings_only_namespace_fallback_shows_namespaced_resources_in_navigation`, `namespaced_resource_link_stays_hidden_when_cached_config_flag_is_false`, and `configured_namespaced_resource_link_is_visible_without_namespace_listing_access`.
- Incremental links, category reuse, ordering, and identity preservation: `resource_config_burst_preserves_existing_navigation_nodes`, `resource_navigation_items_appear_incrementally_as_permissions_complete`, `permission_driven_resource_add_keeps_existing_navigation_nodes`, `category_nav_items_follow_alpha_ordering`, `crd_delta_does_not_rebuild_unrelated_resource_nodes`, and `namespace_addition_does_not_replace_namespace_navigation_link`.
- CRD/custom-resource lifecycle: `custom_resource_definition_added_after_navigation_build_adds_custom_resource_entry`, `coalesced_custom_resource_updates_add_each_navigation_entry`, `custom_resource_definition_update_updates_existing_navigation_entry_without_replacing_group`, `custom_resource_definition_delete_removes_navigation_entry_without_rebuilding_remaining_groups`, and `custom_resource_definition_delete_prunes_empty_group_branch_without_replacing_root`.
- Port-forward visibility: `port_forwarders_is_under_network_category_not_top_level`, `port_forwarders_is_hidden_when_pod_portforward_is_not_allowed`, `initial_navigation_build_does_not_check_port_forward_until_pod_permissions_are_loaded`, and `resource_navigation_updates_incrementally_and_port_forward_waits_for_pod_permissions`.
- Resource count lifecycle: `selecting_unseeded_resource_navigation_link_keeps_count_blank`, `first_click_on_resource_navigation_link_shows_count`, `selecting_seeded_resource_navigation_link_shows_source_cache_count`, `resource_navigation_count_updates_when_events_arrive_after_initial_zero`, `resource_navigation_count_updates_when_runtime_is_decorated`, `resource_navigation_count_is_preserved_until_resource_is_seeded`, and `event_navigation_count_recovers_when_event_seed_happened_before_namespace_permission`.
- Selection and document behavior: `selecting_pods_in_limited_access_cluster_opens_populated_resource_list`, `resource_context_menu_open_new_tab_creates_distinct_document_id`, `updated_crd_reopening_resource_list_document_uses_the_new_generated_type`, and `stale_crd_navigation_link_opens_the_current_generated_resource_type`.
- Visual rendering of count badges: `NavigationViewTests.resource_count_assigned_after_template_creation_is_rendered`.
