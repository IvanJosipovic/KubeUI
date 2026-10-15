# MR Diff Detection

- The feature is available only when Crossplane Provider resources are present in the connected cluster.
- Provider selection owns the pod-log monitor lifecycle; changing providers or disposing the view must cancel and await the previous monitor.
- Read retained logs from all matching provider pods and continue following current container logs. Detect container restarts for the same pod and resume monitoring without clearing accumulated rows.
- Parse only `Diff detected` records and expose structured rows through `DynamicTableView`; keep log parsing details out of the view. Recover only complete attributes from truncated log lines and report that the displayed data is incomplete.
- Pass records through a bounded channel so slow row aggregation applies backpressure rather than growing memory without limit or silently dropping records.
- Bound retained distinct rows to 50,000 and visibly report when the cap is reached; existing rows may continue to update until the user clears the results.
- Keep aggregation off the UI thread and publish changes through `DynamicTableViewSource`. Refresh only dirty diff groups; do not copy every row in a group for each incoming record.
- Reset monitor line deduplication when the displayed rows are cleared, so retained logs can repopulate the table.
- Keep search, selection, sorting, filtering, and YAML context-menu behavior on the DynamicTable contracts. Test filtering/search/sorting and keyed selection through the source, not through obsolete DataGrid adapters.
