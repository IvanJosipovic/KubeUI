# DynamicTableView Benchmarks

- Keep this project independent from KubeUI application and test projects; reference only the library and BenchmarkDotNet.
- Keep one top-level type per C# file and group benchmark code by operation.
- Cover representative 100, 1,000, and 10,000 row sources and a 20-column case.
- Measure allocations as well as latency. Use Release builds and compare repeated runs on the same machine.
- Do not add correctness assertions here; those belong in the separate test project.
