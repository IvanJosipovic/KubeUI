using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.App.Infrastructure.Platform;
using KubeUI.Features.Resources.Workloads.v1.Pod;
using KubeUI.Kubernetes;
using Moq;
using Shouldly;

namespace KubeUI.Features.Resources.Tests.Workloads.v1.Pod;

public sealed class PortForwarderListViewModelTests
{
    [AvaloniaFact]
    public async Task Initialize_binds_cluster_port_forwarders_and_tracks_additions()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        using var viewModel = CreateViewModel();
        viewModel.Initialize(workspace);

        var first = CreateForwarder(workspace.Runtime, "default", "api", 8080, localPort: 18080);
        var second = CreateForwarder(workspace.Runtime, "other", "worker", 9090, localPort: 19090);
        workspace.Runtime.PortForwarders.Add(first);

        await WaitUntilAsync(() => Items(viewModel).Length == 1);
        workspace.Runtime.PortForwarders.Add(second);
        await WaitUntilAsync(() => Items(viewModel).Length == 2);

        Items(viewModel).Select(forwarder => forwarder.Name).ShouldBe(["api", "worker"], ignoreOrder: false);
    }

    [AvaloniaFact]
    public async Task Remove_with_confirmed_dialog_removes_the_forwarder_from_the_cluster()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        var dialogs = new List<ContentDialogSettings>();
        using var viewModel = CreateViewModel(dialogs, result: FAContentDialogResult.Primary);
        viewModel.Initialize(workspace);
        var forwarder = CreateForwarder(workspace.Runtime, "default", "api", 8080, localPort: 18080);
        workspace.Runtime.PortForwarders.Add(forwarder);
        await WaitUntilAsync(() => Items(viewModel).Length == 1);

        viewModel.RemoveCommand.Execute(forwarder);
        await WaitUntilAsync(() => Items(viewModel).Length == 0);

        var dialog = dialogs.ShouldHaveSingleItem();
        dialog.Content!.ToString().ShouldContain("default/api:8080");
        workspace.Runtime.PortForwarders.ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task Remove_with_declined_dialog_keeps_the_forwarder()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        using var viewModel = CreateViewModel(result: FAContentDialogResult.Secondary);
        viewModel.Initialize(workspace);
        var forwarder = CreateForwarder(workspace.Runtime, "default", "api", 8080, localPort: 18080);
        workspace.Runtime.PortForwarders.Add(forwarder);
        await WaitUntilAsync(() => Items(viewModel).Length == 1);

        await viewModel.RemoveCommand.ExecuteAsync(forwarder);

        Items(viewModel).Length.ShouldBe(1);
        workspace.Runtime.PortForwarders.Count.ShouldBe(1);
    }

    private static PortForwarderListViewModel CreateViewModel(
        List<ContentDialogSettings>? dialogs = null,
        FAContentDialogResult result = FAContentDialogResult.Primary)
    {
        var manager = new Mock<IDialogManager>();
        manager.Setup(x => x.ShowFrameworkDialogAsync(
                It.IsAny<System.ComponentModel.INotifyPropertyChanged?>(),
                It.IsAny<ContentDialogSettings>(),
                It.IsAny<Func<object?, string>?>()))
            .Callback<System.ComponentModel.INotifyPropertyChanged?, ContentDialogSettings, Func<object?, string>?>(
                (_, settings, _) => dialogs?.Add(settings))
            .ReturnsAsync(result);
        return new PortForwarderListViewModel(
            new DialogService(manager.Object),
            Application.Current.GetRequiredTestService<IPlatformServices>());
    }

    private static PortForwarder CreateForwarder(
        IClusterRuntime runtime,
        string @namespace,
        string podName,
        int containerPort,
        int localPort)
    {
        var forwarder = new PortForwarder(runtime, @namespace, localPort);
        forwarder.SetPod(podName, containerPort);
        forwarder.Status = "Active";
        forwarder.Connections = 0;
        return forwarder;
    }

    private static PortForwarder[] Items(PortForwarderListViewModel viewModel)
        => viewModel.TableSource.Items.Cast<PortForwarder>().ToArray();

    private static Task WaitUntilAsync(Func<bool> condition, int timeoutMilliseconds = 5000)
        => TestWait.UntilAsync(
            condition,
            timeoutMilliseconds,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());
}
