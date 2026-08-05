using System.Globalization;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Xaml.Interactivity;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Avalonia.Themes.Fluent;
using FluentAvalonia.Styling;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using Semi.Avalonia;
using Ursa.Controls;
using Ursa.Themes.Semi;
using Westermo.GraphX.Controls.Avalonia.Themes.Fluent;
using AvaloniaStyles = Avalonia.Styling.Styles;
using LayoutOrientation = Avalonia.Layout.Orientation;

namespace KubeUI.Avalonia.Styles;

public sealed class Fluent : AvaloniaStyles
{
    private static readonly CultureInfo s_locale = CultureInfo.CurrentUICulture;
    private static readonly Uri s_baseUri = new("avares://KubeUI.Avalonia");

    public Fluent()
    {
        Add(new SemiTheme { Locale = s_locale });
        Add(new UrsaSemiTheme { Locale = s_locale });
        Add(new GraphXFluentTheme());
        Add(new FluentAvaloniaTheme());
        Add(CreateFluentTheme());
        Add(CreateTypographyResources());
        Add(CreateThemeResourceStyles(CreateSemanticLightResources(), CreateSemanticDarkResources()));
        Add(CreateThemeResourceStyles(CreateVisualizationLightResources(), CreateVisualizationDarkResources()));
        Add(CreateStyleInclude("avares://Avalonia.Controls.DataGrid/Themes/Fluent.v2.xaml"));
        Add(CreateStyleInclude("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml"));
        Add(new DockFluentTheme());
        Add(CreateStyleInclude("avares://SvcSystems.UI.Terminal/Styles/Colors.axaml"));
        DataGridStyles.AddTo(this);

        Add(new Style<DocumentControl>()
            .Setter(DocumentControl.HeaderTemplateProperty, new FuncDataTemplate<IDockable>((dockable, _) => CreateDocumentHeader(dockable!), false)));

        Add(new Style<MultiComboBoxItem>()
            .Setter(global::Avalonia.Controls.Primitives.TemplatedControl.PaddingProperty, new Thickness(4, 2, 4, 2))
            .Setter(Layoutable.WidthProperty, 200d)
            .Setter(Layoutable.MinHeightProperty, 22d));

        Add(new Style<MultiComboBoxSelectedItemList>()
            .Setter(Interaction.BehaviorsProperty,
                new BehaviorCollectionTemplate
                {
                    Content = (IServiceProvider? _) =>
                        new TemplateResult<BehaviorCollection>(
                        [new ToggleMultiComboBoxBehavior()],
                        new NameScope())
                }));

        Add(new Style<HostWindow>(x => x.OfType<HostWindow>().Class(":toolwindow"))
            .Background(new DynamicResourceExtension("SystemRegionBrush"))
            .Opacity(1d)
            .RequestedThemeVariant(CompiledBinding.Create<Application, ThemeVariant?>(x => x.RequestedThemeVariant, source: Application.Current))
            .TransparencyLevelHint([WindowTransparencyLevel.None]));
    }

    private static FluentTheme CreateFluentTheme()
    {
        var theme = new FluentTheme
        {
            DensityStyle = DensityStyle.Compact
        };

        theme.Palettes.Add(ThemeVariant.Light, CreateLightPalette());
        theme.Palettes.Add(ThemeVariant.Dark, CreateDarkPalette());

        return theme;
    }

    private static AvaloniaStyles CreateThemeResourceStyles(
        ResourceDictionary lightResources,
        ResourceDictionary darkResources)
    {
        return new AvaloniaStyles
        {
            Resources = new ResourceDictionary
            {
                ThemeDictionaries =
                {
                    [ThemeVariant.Light] = lightResources,
                    [ThemeVariant.Dark] = darkResources,
                }
            }
        };
    }

    private static AvaloniaStyles CreateTypographyResources()
    {
        var styles = new AvaloniaStyles
        {
            Resources = new ResourceDictionary
            {
                [Typography.AppFontFamilyResourceKey] = new FontFamily(Typography.AppFontFamilyName),
                [Typography.AppFontSizeResourceKey] = Typography.DefaultAppFontSize,
                [Typography.CodeFontFamilyResourceKey] = new FontFamily(Typography.CodeFontFamilyName),
                [Typography.CodeFontSizeResourceKey] = Typography.DefaultCodeFontSize,
                [Typography.TitleFontSizeResourceKey] = Typography.DefaultTitleFontSize
            }
        };

        styles.Add(new Style<Control>()
            .Setter(TextElement.FontFamilyProperty, new DynamicResourceExtension(Typography.AppFontFamilyResourceKey))
            .Setter(TextElement.FontSizeProperty, new DynamicResourceExtension(Typography.AppFontSizeResourceKey)));

        return styles;
    }

    private static ResourceDictionary CreateSemanticLightResources() => new()
    {
        ["PodStatusReadyBrush"] = Brush("#2E7D32"),
        ["PodStatusWarningBrush"] = Brush("#B35A00"),
        ["ContainerStatusReadyBrush"] = Brush("#2E7D32"),
        ["ContainerStatusInitReadyBrush"] = Brush("#7B3FB5"),
        ["ContainerStatusEphemeralReadyBrush"] = Brush("#0067B8"),
        ["ContainerStatusRunningBrush"] = Brush("#B35A00"),
        ["ContainerStatusInitRunningBrush"] = Brush("#9C4DCC"),
        ["ContainerStatusEphemeralRunningBrush"] = Brush("#2F80C0"),
        ["ContainerStatusWaitingBrush"] = Brush("#C62828"),
        ["ContainerStatusInitWaitingBrush"] = Brush("#AD1457"),
        ["ContainerStatusEphemeralWaitingBrush"] = Brush("#D84315"),
        ["ContainerStatusCompletedBrush"] = Brush("#5C6B73"),
        ["ContainerStatusErrorBrush"] = Brush("#C62828"),
        ["SubtleOutlineBrush"] = Brush("#757575"),
    };

    private static ResourceDictionary CreateSemanticDarkResources() => new()
    {
        ["PodStatusReadyBrush"] = Brush("#81C784"),
        ["PodStatusWarningBrush"] = Brush("#FFB454"),
        ["ContainerStatusReadyBrush"] = Brush("#81C784"),
        ["ContainerStatusInitReadyBrush"] = Brush("#C084FC"),
        ["ContainerStatusEphemeralReadyBrush"] = Brush("#4FA3FF"),
        ["ContainerStatusRunningBrush"] = Brush("#FFB454"),
        ["ContainerStatusInitRunningBrush"] = Brush("#D18BFF"),
        ["ContainerStatusEphemeralRunningBrush"] = Brush("#78B8FF"),
        ["ContainerStatusWaitingBrush"] = Brush("#FF6B6B"),
        ["ContainerStatusInitWaitingBrush"] = Brush("#FF80AB"),
        ["ContainerStatusEphemeralWaitingBrush"] = Brush("#FF8A65"),
        ["ContainerStatusCompletedBrush"] = Brush("#B8C4CC"),
        ["ContainerStatusErrorBrush"] = Brush("#FF6B6B"),
        ["SubtleOutlineBrush"] = Brush("#9E9E9E"),
    };

    private static ResourceDictionary CreateVisualizationLightResources() => new()
    {
        ["VisualizationRelationshipOwnerBrush"] = Brush("#0067B8"),
        ["VisualizationRelationshipReferenceBrush"] = Brush("#5C6B73"),
        ["VisualizationRelationshipSelectorBrush"] = Brush("#7B3FB5"),
        ["VisualizationRelationshipLabelBrush"] = Brush("#B35A00"),
        ["VisualizationRelationshipStorageBrush"] = Brush("#008577"),
        ["VisualizationRelationshipIdentityBrush"] = Brush("#2E7D32"),
        ["VisualizationRelationshipRbacBrush"] = Brush("#B0005A"),
        ["VisualizationRelationshipEventBrush"] = Brush("#A15C00"),
        ["VisualizationRelationshipGitOpsBrush"] = Brush("#C2185B"),
        ["VisualizationRelationshipDefaultBrush"] = Brush("#5C6B73"),
    };

    private static ResourceDictionary CreateVisualizationDarkResources() => new()
    {
        ["VisualizationRelationshipOwnerBrush"] = Brush("#4FA3FF"),
        ["VisualizationRelationshipReferenceBrush"] = Brush("#B8C4CC"),
        ["VisualizationRelationshipSelectorBrush"] = Brush("#C084FC"),
        ["VisualizationRelationshipLabelBrush"] = Brush("#FFB454"),
        ["VisualizationRelationshipStorageBrush"] = Brush("#40D9C0"),
        ["VisualizationRelationshipIdentityBrush"] = Brush("#81C784"),
        ["VisualizationRelationshipRbacBrush"] = Brush("#FF6FB5"),
        ["VisualizationRelationshipEventBrush"] = Brush("#FFD166"),
        ["VisualizationRelationshipGitOpsBrush"] = Brush("#FF80AB"),
        ["VisualizationRelationshipDefaultBrush"] = Brush("#B8C4CC"),
    };

    private static ColorPaletteResources CreateLightPalette() => new()
    {
        Accent = Color("#ff0073cf"),
        ErrorText = Color("#B00020"),
        AltHigh = Color("#FFFFFF"),
        AltLow = Color("#FFFFFF"),
        AltMedium = Color("#FFFFFF"),
        AltMediumHigh = Color("#FFFFFF"),
        AltMediumLow = Color("#FFFFFF"),
        BaseHigh = Color("#000000"),
        BaseLow = Color("#ffcccccc"),
        BaseMedium = Color("#ff898989"),
        BaseMediumHigh = Color("#ff5d5d5d"),
        BaseMediumLow = Color("#ff737373"),
        ChromeAltLow = Color("#ff5d5d5d"),
        ChromeBlackHigh = Color("#000000"),
        ChromeBlackLow = Color("#ffcccccc"),
        ChromeBlackMedium = Color("#ff5d5d5d"),
        ChromeBlackMediumLow = Color("#ff898989"),
        ChromeDisabledHigh = Color("#ffcccccc"),
        ChromeDisabledLow = Color("#ff898989"),
        ChromeGray = Color("#ff737373"),
        ChromeHigh = Color("#ffcccccc"),
        ChromeLow = Color("#ffececec"),
        ChromeMedium = Color("#ffe6e6e6"),
        ChromeMediumLow = Color("#ffececec"),
        ChromeWhite = Color("#FFFFFF"),
        ListLow = Color("#ffe6e6e6"),
        ListMedium = Color("#ffcccccc"),
        RegionColor = Color("#EEEEF2")
    };

    private static ColorPaletteResources CreateDarkPalette() => new()
    {
        Accent = Color("#ff0073cf"),
        ErrorText = Color("#FF8A80"),
        AltHigh = Color("#000000"),
        AltLow = Color("#000000"),
        AltMedium = Color("#000000"),
        AltMediumHigh = Color("#000000"),
        AltMediumLow = Color("#000000"),
        BaseHigh = Color("#FFFFFF"),
        BaseLow = Color("#ff333333"),
        BaseMedium = Color("#ff9a9a9a"),
        BaseMediumHigh = Color("#ffb4b4b4"),
        BaseMediumLow = Color("#ff676767"),
        ChromeAltLow = Color("#ffb4b4b4"),
        ChromeBlackHigh = Color("#000000"),
        ChromeBlackLow = Color("#ffb4b4b4"),
        ChromeBlackMedium = Color("#000000"),
        ChromeBlackMediumLow = Color("#000000"),
        ChromeDisabledHigh = Color("#ff333333"),
        ChromeDisabledLow = Color("#ff9a9a9a"),
        ChromeGray = Color("#808080"),
        ChromeHigh = Color("#808080"),
        ChromeLow = Color("#ff151515"),
        ChromeMedium = Color("#ff1d1d1d"),
        ChromeMediumLow = Color("#ff2c2c2c"),
        ChromeWhite = Color("#FFFFFF"),
        ListLow = Color("#ff1d1d1d"),
        ListMedium = Color("#ff333333"),
        RegionColor = Color("#1E1E1E")
    };

    private static StyleInclude CreateStyleInclude(string source) => new(s_baseUri) { Source = new Uri(source) };

    private static Color Color(string value) => global::Avalonia.Media.Color.Parse(value);

    private static SolidColorBrush Brush(string value) => new(Color(value));

    private static StackPanel CreateDocumentHeader(IDockable dockable)
    {
        var cluster = GetCluster(dockable);

        return new StackPanel()
            .VerticalAlignment(VerticalAlignment.Center)
            .Orientation(LayoutOrientation.Horizontal)
            .Children(
                CreateClusterIndicator(cluster),
                new TextBlock()
                    .VerticalAlignment(VerticalAlignment.Center)
                    .BindValue(TextBlock.TextProperty, CompiledBinding.Create<IDockable, string?>(x => x.Title, source: dockable)));
    }

    private static Rectangle CreateClusterIndicator(ClusterWorkspace? cluster)
    {
        var indicator = new Rectangle()
            .Width(10)
            .Height(10)
            .Margin(0, 0, 2, 0)
            .IsVisible(cluster != null)
            .Stroke(Brushes.Gray)
            .StrokeThickness(0.6);

        return cluster == null
            ? indicator
            : indicator
                .Fill(cluster, x => x.ClusterColor)
                .ToolTip_Tip(cluster, x => x.Runtime.Name);
    }

    private static ClusterWorkspace? GetCluster(IDockable dockable)
    {
        return dockable.GetType().GetProperty("Cluster")?.GetValue(dockable) as ClusterWorkspace;
    }
}
