using Avalonia.Headless;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: AvaloniaTestApplication(typeof(KubeUI.DynamicTableView.Tests.Infra.HeadlessTestAppBuilder))]
