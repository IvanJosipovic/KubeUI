using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;

namespace KubeUI.Avalonia.Features.Resources.Common;

public sealed partial class MenuItemViewModel : ObservableObject
{
    private string? _header;
    private ICommand? _command;
    private object? _commandParameter;
    private AvaloniaList<MenuItemViewModel>? _items;
    private INotifyCollectionChanged? _itemsCollection;
    private string? _iconResource;
    private Icon? _fluentIcon;
    private bool _isSeparator;

    public string? Header
    {
        get => _header;
        set => SetProperty(ref _header, value);
    }

    public ICommand? Command
    {
        get => _command;
        set
        {
            if (SetProperty(ref _command, value))
            {
                OnPropertyChanged(nameof(IsVisible));
            }
        }
    }

    public object? CommandParameter
    {
        get => _commandParameter;
        set => SetProperty(ref _commandParameter, value);
    }

    public AvaloniaList<MenuItemViewModel>? Items
    {
        get => _items;
        set
        {
            if (SetProperty(ref _items, value))
            {
                UpdateItemsSubscription(value);
                OnPropertyChanged(nameof(IsVisible));
            }
        }
    }

    public string? IconResource
    {
        get => _iconResource;
        set => SetProperty(ref _iconResource, value);
    }

    public Icon? FluentIcon
    {
        get => _fluentIcon;
        set => SetProperty(ref _fluentIcon, value);
    }

    public bool IsSeparator
    {
        get => _isSeparator;
        set
        {
            if (SetProperty(ref _isSeparator, value))
            {
                OnPropertyChanged(nameof(IsVisible));
            }
        }
    }

    public bool IsVisible => IsSeparator || Command != null || (Items?.Count ?? 0) > 0;

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
