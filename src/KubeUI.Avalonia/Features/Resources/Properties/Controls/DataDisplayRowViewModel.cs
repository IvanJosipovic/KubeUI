using KubeUI.Avalonia.Infrastructure.Presentation;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

internal sealed partial class DataDisplayRowViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Key { get; set; }

    [ObservableProperty]
    public partial string Value { get; set; }

    internal string? OriginalKey { get; }

    public DataDisplayRowViewModel(string key, string value, string? originalKey = null)
    {
        Key = key;
        Value = value;
        OriginalKey = originalKey;
    }
}
