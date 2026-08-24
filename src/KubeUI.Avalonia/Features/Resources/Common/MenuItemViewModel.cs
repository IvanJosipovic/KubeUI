using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia.Collections;
using FluentIcons.Common;

namespace KubeUI.Avalonia.Features.Resources.Common;

public sealed partial class MenuItemViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string? Title { get; set; }

    [ObservableProperty]
    public partial ICommand? Command { get; set; }

    [ObservableProperty]
    public partial object? CommandParameter { get; set; }

    [ObservableProperty]
    public partial AvaloniaList<MenuItemViewModel>? Items { get; set; }

    private INotifyCollectionChanged? _itemsCollection;

    [ObservableProperty]
    public partial string? IconResource { get; set; }

    [ObservableProperty]
    public partial Icon? FluentIcon { get; set; }

    [ObservableProperty]
    public partial bool IsSeparator { get; set; }

    [ObservableProperty]
    public partial bool ShowInPropertiesView { get; set; } = true;

    public bool IsVisible => IsSeparator || Command != null || (Items?.Count ?? 0) > 0;

    partial void OnCommandChanged(ICommand? value)
    {
        OnPropertyChanged(nameof(IsVisible));
    }

    partial void OnItemsChanged(AvaloniaList<MenuItemViewModel>? value)
    {
        UpdateItemsSubscription(value);
        OnPropertyChanged(nameof(IsVisible));
    }

    partial void OnIsSeparatorChanged(bool value)
    {
        OnPropertyChanged(nameof(IsVisible));
    }

    private void ItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(IsVisible));
    }

    private void UpdateItemsSubscription(AvaloniaList<MenuItemViewModel>? items)
    {
        if (_itemsCollection != null)
        {
            _itemsCollection.CollectionChanged -= ItemsCollectionChanged;
            _itemsCollection = null;
        }

        if (items is INotifyCollectionChanged collection)
        {
            _itemsCollection = collection;
            _itemsCollection.CollectionChanged += ItemsCollectionChanged;
        }
    }
}
