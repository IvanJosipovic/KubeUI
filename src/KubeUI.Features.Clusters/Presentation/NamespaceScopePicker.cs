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
public sealed class NamespaceScopePicker : UserControl
{
    /// <summary>Identifies <see cref="Namespaces"/>.</summary>
    public static readonly DirectProperty<NamespaceScopePicker, IEnumerable?> NamespacesProperty =
        AvaloniaProperty.RegisterDirect<NamespaceScopePicker, IEnumerable?>(nameof(Namespaces), x => x.Namespaces, (x, value) => x.Namespaces = value);

    /// <summary>Identifies <see cref="SelectedNamespaces"/>.</summary>
    public static readonly DirectProperty<NamespaceScopePicker, IList?> SelectedNamespacesProperty =
        AvaloniaProperty.RegisterDirect<NamespaceScopePicker, IList?>(nameof(SelectedNamespaces), x => x.SelectedNamespaces, (x, value) => x.SelectedNamespaces = value);

    /// <summary>Identifies <see cref="IsSelectionLinked"/>.</summary>
    public static readonly DirectProperty<NamespaceScopePicker, bool> IsSelectionLinkedProperty =
        AvaloniaProperty.RegisterDirect<NamespaceScopePicker, bool>(nameof(IsSelectionLinked), x => x.IsSelectionLinked, (x, value) => x.IsSelectionLinked = value, unsetValue: true);

    /// <summary>Identifies <see cref="IsSelectionEnabled"/>.</summary>
    public static readonly DirectProperty<NamespaceScopePicker, bool> IsSelectionEnabledProperty =
        AvaloniaProperty.RegisterDirect<NamespaceScopePicker, bool>(nameof(IsSelectionEnabled), x => x.IsSelectionEnabled, (x, value) => x.IsSelectionEnabled = value, unsetValue: true);

    /// <summary>Identifies <see cref="IsLinkEnabled"/>.</summary>
    public static readonly DirectProperty<NamespaceScopePicker, bool> IsLinkEnabledProperty =
        AvaloniaProperty.RegisterDirect<NamespaceScopePicker, bool>(nameof(IsLinkEnabled), x => x.IsLinkEnabled, (x, value) => x.IsLinkEnabled = value, unsetValue: true);

    /// <summary>Identifies <see cref="PickerWidth"/>.</summary>
    public static readonly DirectProperty<NamespaceScopePicker, double> PickerWidthProperty =
        AvaloniaProperty.RegisterDirect<NamespaceScopePicker, double>(nameof(PickerWidth), x => x.PickerWidth, (x, value) => x.PickerWidth = value, unsetValue: 200);

    private IEnumerable? _namespaces;
    private IList? _selectedNamespaces;
    private bool _isSelectionLinked = true;
    private bool _isSelectionEnabled = true;
    private bool _isLinkEnabled = true;
    private double _pickerWidth = 200;

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
            PlaceholderText = L10n.Instance.NamespaceScopePicker_SelectNamespace.CurrentValue()!,
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
        ToolTip.SetTip(link, L10n.Instance.NamespaceScopePicker_LinkNamespaces.CurrentValue()!);
        link.Bind(ToggleButton.IsCheckedProperty, CompiledBinding.Create<NamespaceScopePicker, bool>(x => x.IsSelectionLinked, source: this, mode: BindingMode.TwoWay));
        link.Bind(IsEnabledProperty, CompiledBinding.Create<NamespaceScopePicker, bool>(x => x.IsLinkEnabled, source: this));

        Content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { picker, link }
        };
    }

    /// <summary>Gets or sets the available <see cref="V1Namespace"/> items.</summary>
    public IEnumerable? Namespaces
    {
        get => _namespaces;
        set => SetAndRaise(NamespacesProperty, ref _namespaces, value);
    }

    /// <summary>
    /// Gets or sets the caller-owned selection list. The picker uses this same instance;
    /// replacing it retargets selection without copying or assigning meaning to an empty list.
    /// </summary>
    public IList? SelectedNamespaces
    {
        get => _selectedNamespaces;
        set => SetAndRaise(SelectedNamespacesProperty, ref _selectedNamespaces, value);
    }

    /// <summary>Gets or sets whether selection is linked. Defaults to true; callers implement linking policy.</summary>
    public bool IsSelectionLinked
    {
        get => _isSelectionLinked;
        set => SetAndRaise(IsSelectionLinkedProperty, ref _isSelectionLinked, value);
    }

    /// <summary>Gets or sets whether namespace selection is enabled, independently of linking. Defaults to true.</summary>
    public bool IsSelectionEnabled
    {
        get => _isSelectionEnabled;
        set => SetAndRaise(IsSelectionEnabledProperty, ref _isSelectionEnabled, value);
    }

    /// <summary>Gets or sets whether the link toggle is enabled, independently of selection. Defaults to true.</summary>
    public bool IsLinkEnabled
    {
        get => _isLinkEnabled;
        set => SetAndRaise(IsLinkEnabledProperty, ref _isLinkEnabled, value);
    }

    /// <summary>Gets or sets the multi-select width, excluding the link toggle. Defaults to 200 pixels.</summary>
    public double PickerWidth
    {
        get => _pickerWidth;
        set => SetAndRaise(PickerWidthProperty, ref _pickerWidth, value);
    }
}
