# Cluster Error

## Current Behavior
- Cluster connection and exec errors are surfaced through a shared cluster error document.
- Reopening an error reuses the same document id instead of creating duplicates.
