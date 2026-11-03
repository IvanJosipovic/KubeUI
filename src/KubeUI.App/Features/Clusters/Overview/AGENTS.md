# Cluster Overview

## Current Behavior
- The overview screen refreshes pod and node data from the selected cluster workspace.
- The embedded events list is initialized against the same cluster workspace.
- CPU, memory and pod gauges (LiveCharts `XamlGaugeSeries`) draw arcs with flat ends. The LiveCharts default theme rounds gauge arcs (`CornerRadius = 8`) and overrides values set on the series, so the flat ends come from a `HasRuleForGaugeSeries` rule in `App.Initialize()`, not from the series properties.
