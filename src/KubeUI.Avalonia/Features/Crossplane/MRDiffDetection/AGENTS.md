# MR Diff Detection

- The feature is available only when Crossplane Provider resources are present in the connected cluster.
- Provider selection owns the pod-log monitor lifecycle; changing or disposing the view must cancel the previous monitor.
- Read retained logs from all matching provider pods and continue following current container logs.
- Parse only `Diff detected` records and expose structured rows through the ProDataGrid; do not surface raw log parsing details in the view.
- Apply collection updates on the Avalonia dispatcher and keep parsing independent of rendering.
