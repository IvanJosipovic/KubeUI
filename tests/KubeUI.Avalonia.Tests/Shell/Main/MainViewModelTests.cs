using System.Linq;
using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using KubeUI.Avalonia.Shell.Documents.CloudClusters.Aks.ViewModels;
using KubeUI.Avalonia.Shell.Main.ViewModels;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Shell.Main;

public sealed class MainViewModelTests : AvaloniaTestBase
{
    private MainViewModel CreateViewModel()
    {
        return TestApp.CurrentServices?.GetRequiredService<MainViewModel>()
            ?? throw new InvalidOperationException("Test services are not initialized.");
    }

    [AvaloniaFact]
    public async Task load_aks_clusters_command_opens_docked_aks_assistant()
    {
        var vm = CreateViewModel();
        var documents = TestApp.CurrentServices!.GetRequiredService<IFactory>().GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        vm.ImportAksClusterCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        documents.VisibleDockables!
            .OfType<ImportAksClusterViewModel>()
            .Count()
            .ShouldBe(1);
    }

}
