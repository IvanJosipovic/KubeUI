using System.Collections;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using k8s.Models;
using Ursa.Controls;

namespace KubeUI.Features.Clusters.Presentation;

/// <summary>
/// Presents caller-owned namespace selection and linking without interpreting scope semantics.
/// Callers control visibility through <see cref="Visual.IsVisible"/> and disable selection and
/// linking independently for read-only scopes.
/// </summary>
public sealed partial class NamespaceScopePicker : UserControl
{
    /// <summary>Gets or sets the available <see cref="V1Namespace"/> items.</summary>
    [GeneratedDirectProperty]
    public partial IEnumerable? Namespaces { get; set; }

    /// <summary>
    /// Gets or sets the caller-owned selection list. The picker uses this same instance;
    /// replacing it retargets selection without copying or assigning meaning to an empty list.
    /// </summary>
    [GeneratedDirectProperty]
    public partial IList? SelectedNamespaces { get; set; }

    /// <summary>Gets or sets whether selection is linked. Defaults to true; callers implement linking policy.</summary>
    [GeneratedDirectProperty(UnsetValue = true)]
    public partial bool IsSelectionLinked { get; set; } = true;

    /// <summary>Gets or sets whether namespace selection is enabled, independently of linking. Defaults to true.</summary>
    [GeneratedDirectProperty(UnsetValue = true)]
    public partial bool IsSelectionEnabled { get; set; } = true;

    /// <summary>Gets or sets whether the link toggle is enabled, independently of selection. Defaults to true.</summary>
    [GeneratedDirectProperty(UnsetValue = true)]
    public partial bool IsLinkEnabled { get; set; } = true;

    /// <summary>Gets or sets the multi-select width, excluding the link toggle. Defaults to 200 pixels.</summary>
    [GeneratedDirectProperty(UnsetValue = 200d)]
    public partial double PickerWidth { get; set; } = 200;

    /// <summary>Creates a compact namespace multi-select and adjacent link toggle.</summary>
    public NamespaceScopePicker()
    {
        FuncDataTemplate<V1Namespace> template = new((ns, _) => new TextBlock
        {
            Text = ns?.Metadata?.Name ?? string.Empty
        });
        MultiComboBox picker = new()
        {
            MinWidth = 140,
            MaxHeight = 20,
            Margin = new Thickness(0, 0, 2, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            PlaceholderText = ClustersLanguageManager.Instance.NamespaceScopePicker_SelectNamespace.CurrentValue()!,
            ItemTemplate = template,
            SelectedItemTemplate = template
        };
        picker.Classes.Add("ClearButton");
        picker.Bind(WidthProperty, CompiledBinding.Create<NamespaceScopePicker, double>(x => x.PickerWidth, source: this));
        picker.Bind(ItemsControl.ItemsSourceProperty, CompiledBinding.Create<NamespaceScopePicker, IEnumerable?>(x => x.Namespaces, source: this));
        picker.Bind(MultiComboBox.SelectedItemsProperty, CompiledBinding.Create<NamespaceScopePicker, IList?>(x => x.SelectedNamespaces, source: this, mode: BindingMode.OneWay));
        picker.Bind(IsEnabledProperty, CompiledBinding.Create<NamespaceScopePicker, bool>(x => x.IsSelectionEnabled, source: this));

        ToggleButton link = new()
        {
            Margin = new Thickness(0, 0, 2, 0),
            Content = new FluentIcon { Icon = Icon.Link }
        };
        ToolTip.SetTip(link, ClustersLanguageManager.Instance.NamespaceScopePicker_LinkNamespaces.CurrentValue()!);
        link.Bind(ToggleButton.IsCheckedProperty, CompiledBinding.Create<NamespaceScopePicker, bool>(x => x.IsSelectionLinked, source: this, mode: BindingMode.TwoWay));
        link.Bind(IsEnabledProperty, CompiledBinding.Create<NamespaceScopePicker, bool>(x => x.IsLinkEnabled, source: this));

        Content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { picker, link }
        };
    }

}
