using KubeUI.Avalonia.Resources.Workloads.v1.Pod;
using Avalonia.Headless.XUnit;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Workloads.Pod;

public sealed class PodLogsViewModelTests
{
    [AvaloniaFact]
    public void append_log_keeps_only_the_newest_entries()
    {
        var logger =
            Application.Current.GetTestServices().GetRequiredService<ILogger<PodLogsViewModel>>();
        using PodLogsViewModel viewModel = new(logger);

        for (var index = 0; index < PodLogsViewModel.MaxLogEntries + 1; index++)
        {
            viewModel.AppendLog($"log-{index}");
        }

        viewModel.Logs.Text.ShouldNotContain("log-0");
        viewModel.Logs.Text.ShouldStartWith("log-1" + Environment.NewLine);
        viewModel.Logs.Text.ShouldContain($"log-{PodLogsViewModel.MaxLogEntries}");
        viewModel.Logs.LineCount.ShouldBe(PodLogsViewModel.MaxLogEntries + 1);
    }
}
