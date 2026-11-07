using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using Helm.NET;
using KubeUI.Features.Helm;
using KubeUI.Features.Helm.Helm;
using KubeUI.Features.Helm.Repositories;
using KubeUI.Kubernetes;
using Moq;
using Shouldly;

namespace KubeUI.Features.Helm.Tests.Repositories;

public sealed class HelmRepositoriesViewTests
{
    [AvaloniaFact]
    public async Task Header_retains_localized_count_status_and_repository_commands()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new HelmRepository("test", "https://example.test")]);
        helm.Setup(service => service.UpdateRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        using var viewModel = new HelmRepositoriesViewModel(helm.Object, Mock.Of<IDialogService>(),
            Mock.Of<IHelmDocumentOpener>(), Mock.Of<IHelmListNotifier>());
        viewModel.Initialize(workspace);
        await viewModel.RefreshCommand.ExecutionTask!.WaitAsync(TestContext.Current.CancellationToken);

        var view = new HelmRepositoriesView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        await WaitForAsync(() => view.GetVisualDescendants().OfType<Label>()
            .Any(label => Equals(label.Content, string.Format(HelmLanguageManager.Instance.HelmRepositoriesView_ItemsFormat.CurrentValue()!, 1))));

        var buttons = view.GetVisualDescendants().OfType<Button>().ToArray();
        var add = buttons.Single(button => ReferenceEquals(button.Command, viewModel.NewRepositoryCommand));
        var update = buttons.Single(button => ReferenceEquals(button.Command, viewModel.UpdateIndexesCommand));
        var refresh = buttons.Single(button => ReferenceEquals(button.Command, viewModel.RefreshCommand));
        add.IsVisible.ShouldBeTrue();
        add.Content.ShouldBeOfType<FluentIcon>().Icon.ShouldBe(Icon.AddSquare);
        update.Content.ShouldBeOfType<FluentIcon>().Icon.ShouldBe(Icon.ArrowDownload);
        refresh.Content.ShouldBeOfType<FluentIcon>().Icon.ShouldBe(Icon.ArrowSync);
        ToolTip.GetTip(add).ShouldBe(HelmLanguageManager.Instance.HelmRepositoriesView_New.CurrentValue());
        ToolTip.GetTip(update).ShouldBe(HelmLanguageManager.Instance.HelmRepositoriesView_UpdateIndexes.CurrentValue());

        Click(window, update);
        await viewModel.UpdateIndexesCommand.ExecutionTask!.WaitAsync(TestContext.Current.CancellationToken);
        await WaitForAsync(() => view.GetVisualDescendants().OfType<TextBlock>()
            .Any(text => text.Text == HelmLanguageManager.Instance.HelmRepositoriesView_IndexesUpdated.CurrentValue()));
        helm.Verify(service => service.UpdateRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()), Times.Once);

        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        Click(window, refresh);
        await viewModel.RefreshCommand.ExecutionTask!.WaitAsync(TestContext.Current.CancellationToken);
        await WaitForAsync(() => view.GetVisualDescendants().OfType<Label>()
            .Any(label => Equals(label.Content, string.Format(HelmLanguageManager.Instance.HelmRepositoriesView_ItemsFormat.CurrentValue()!, 0))));
        viewModel.ItemCount.ShouldBe(0);
        viewModel.Status.ShouldBeEmpty();
    }

    private static void Click(Window window, Button button)
    {
        button.IsEffectivelyVisible.ShouldBeTrue();
        button.IsEffectivelyEnabled.ShouldBeTrue();
        var position = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)
            ?? throw new InvalidOperationException("Toolbar button has no window position.");
        window.MouseDown(position, MouseButton.Left);
        window.MouseUp(position, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static Task WaitForAsync(Func<bool> condition)
        => TestWait.UntilAsync(condition, 5000, TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());
}
