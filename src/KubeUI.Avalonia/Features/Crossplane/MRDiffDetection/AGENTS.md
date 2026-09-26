# MR Diff Detection

- The feature is available only when Crossplane Provider resources are present in the connected cluster.
- Provider selection owns the pod-log monitor lifecycle; changing or disposing the view must cancel the previous monitor.
- Read retained logs from all matching provider pods and continue following current container logs.
- Parse only `Diff detected` records and expose structured rows through the ProDataGrid; do not surface raw log parsing details in the view.
- Update the diff `SourceCache` off the UI thread; use DynamicData `SortAndBind` with `AvaloniaScheduler.Instance` to publish the bound collection on the UI thread. Keep parsing independent of rendering.
