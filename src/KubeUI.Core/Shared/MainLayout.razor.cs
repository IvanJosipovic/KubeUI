using KubeUI.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;

namespace KubeUI.Core.Shared;

public partial class MainLayout
{
    [Inject]
    private ILogger<MainLayout> Logger { get; set; }

    [Inject]
    private IResizeListenerService? ResizeListenerService { get; set; }

    [Inject]
    private Updater Updater { get; set; }

    [Inject]
    private ISnackbar Snackbar { get; set; }

    private bool _drawerOpen = true;
    private bool _drawerOpen2 = false;
    private RenderFragment? menuRenderFragment;
    private string? MenuWidth;
    private ErrorBoundary? errorBoundary;

    private MudTheme Theme = new MudTheme()
    {
        Palette = new Palette()
        {
            Primary = "#326CE5",
            AppbarBackground = "#326CE5"
        }
    };

    protected override async Task OnInitializedAsync()
    {
        ResizeListenerService.OnBreakpointChanged += ResizeListenerService_OnBreakpointChanged;

        ResizeMenu(await ResizeListenerService.GetBreakpoint());

        try
        {
            if (await Updater.UpdateRequired())
            {
                Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopRight;

                var release = await Updater.GetReleases();

                Snackbar.Add($"Update {release.FirstOrDefault()?.tag_name} is available!", Severity.Normal);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking for updates!");
        }
    }

    private void ResizeListenerService_OnBreakpointChanged(object? sender, Breakpoint e)
    {
        ResizeMenu(e);
    }

    private void ResizeMenu(Breakpoint e)
    {
        switch (e)
        {
            case Breakpoint.Xs:
                MenuWidth = "100%";
                break;
            case Breakpoint.Sm:
                MenuWidth = "70%";
                break;
            case Breakpoint.Md:
                MenuWidth = "50%";
                break;
            case Breakpoint.Lg:
                MenuWidth = "50%";
                break;
            case Breakpoint.Xl:
                MenuWidth = "50%";
                break;
        }

        StateHasChanged();
    }

    void DrawerToggle()
    {
        _drawerOpen = !_drawerOpen;
    }

    protected override void OnParametersSet()
    {
        errorBoundary?.Recover();
    }

    public void RenderMenu(RenderFragment renderFragment)
    {
        _drawerOpen2 = true;
        menuRenderFragment = renderFragment;
        StateHasChanged();
    }

    public void HideMenu()
    {
        _drawerOpen2 = false;
        StateHasChanged();
    }
}