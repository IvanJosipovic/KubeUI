using KubeUI.Avalonia.Shell.Main;
using KubeUI.Documentation.Tests.Infra;

namespace KubeUI.Documentation.Tests.Shell;

public sealed class ConnectToClusterRecordingTests
{
    [DocumentationVideoTheory]
    public Task Connect_to_cluster_video(string theme)
    {
        return KubeUIWalkthrough.Create(theme, "connect-to-cluster")
            .FakeCluster("demo-cluster")
            .StartAt<MainView, MainViewModel>(
                x => x.ViewModel.ResetLayoutCommand.Execute(null),
                connectToCluster: false)
            .Speak("Select the cluster in the navigation pane. KubeUI checks access, then loads the resources your account can view.")
            .OpenCluster("demo-cluster")
            .Speak("Now select Pods to browse the workloads running in this cluster.")
            .SelectNavigation("Pods")
            .RecordAsync();
    }
}
