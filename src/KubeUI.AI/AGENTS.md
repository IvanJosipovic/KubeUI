# KubeUI.AI engineering guide

- Keep ACP and `dotacp` protocol details inside this project. Runtime package assets may flow to the executable host because the ACP assemblies must be loadable at runtime, but compile-time protocol assets remain private.
- Expose only KubeUI-owned agent abstractions to application and UI projects.
- Treat agent runtimes as configurable processes; never hardcode runtime-specific behavior into the transport.
- Keep prompts, responses, credentials, kubeconfigs, and Kubernetes Secret contents out of telemetry by default.
- Add adapter and mapping tests for every supported ACP union or update variant.
