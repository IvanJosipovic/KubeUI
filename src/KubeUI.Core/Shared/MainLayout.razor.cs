using KubeUI.Core.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor.Services;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    [Inject]
    private IJSRuntime jSRuntime { get; set; }

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
            var property = typeof(KubernetesJson).GetField("JsonSerializerOptions", BindingFlags.Static | BindingFlags.NonPublic);

            var options = (JsonSerializerOptions)property.GetValue(null);

            options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
            options.Converters.Add(new BoolConverter());
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error setting JsonSerializerOptions");
        }

        try
        {
            if (await Updater.UpdateRequired())
            {
                Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopRight;

                var release = await Updater.GetReleases();

                Snackbar.Add($"Update {release.FirstOrDefault()?.tag_name} is available!", Severity.Success, config =>
                {
                    config.Onclick = async snackbar =>
                    {
                        await jSRuntime.InvokeVoidAsync("open", new object[2] { release.FirstOrDefault().html_url, "_blank" });
                    };
                });
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

    public class BoolConverter : JsonConverter<bool>
    {
        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) =>
            writer.WriteBooleanValue(value);

        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType switch
            {
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                JsonTokenType.String => bool.TryParse(reader.GetString(), out var b) ? b : throw new JsonException(),
                JsonTokenType.Number => reader.TryGetInt64(out long l) ? Convert.ToBoolean(l) : reader.TryGetDouble(out double d) ? Convert.ToBoolean(d) : false,
                _ => throw new JsonException(),
            };
    }
}