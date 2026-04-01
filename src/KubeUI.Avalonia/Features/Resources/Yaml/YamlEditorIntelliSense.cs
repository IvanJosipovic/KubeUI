using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;

namespace KubeUI.Avalonia.Features.Resources.Yaml;

internal sealed class YamlCompletionData(YamlCompletionItemInfo item) : ICompletionData
{
    public string Text => item.Text;

    public object Content => item.Text;

    public object Description => item.Documentation == null
        ? item.Type.Name
        : YamlDocumentationViewFactory.Create(item.Documentation);

    public double Priority => 0;

    public IImage? Image => null;

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        var insertionText = GetInsertionText(textArea, completionSegment);
        textArea.Document.Replace(completionSegment, insertionText);
    }

    private string GetInsertionText(TextArea textArea, ISegment completionSegment)
    {
        if (!RequiresNestedBlock(item.Type))
        {
            return item.InsertionText;
        }

        var line = textArea.Document.GetLineByOffset(completionSegment.Offset);
        var keyIndent = completionSegment.Offset - line.Offset;
        var childIndent = keyIndent + 2;
        var newLine = textArea.Document.Text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        if (IsSequenceType(item.Type))
        {
            return $"{item.Text}:{newLine}{new string(' ', childIndent)}- ";
        }

        return $"{item.Text}:{newLine}{new string(' ', childIndent)}";
    }

    private static bool IsSequenceType(Type type)
    {
        return type != typeof(string)
            && typeof(IEnumerable).IsAssignableFrom(type)
            && !typeof(IDictionary).IsAssignableFrom(type);
    }

    private static bool RequiresNestedBlock(Type type)
    {
        return !IsScalarType(type) && !typeof(IDictionary).IsAssignableFrom(type);
    }

    private static bool IsScalarType(Type type)
    {
        return type == typeof(string)
            || type.IsPrimitive
            || type.IsEnum
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(Guid);
    }
}

internal sealed class YamlDocumentationOverloadProvider : IOverloadProvider
{
    private readonly ObservableCollection<YamlDocumentationInfo> _items = [];
    private int _selectedIndex;

    public YamlDocumentationOverloadProvider(YamlDocumentationInfo documentation)
    {
        _items.Add(documentation);
        _selectedIndex = 0;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            var nextIndex = Math.Clamp(value, 0, Math.Max(0, _items.Count - 1));
            if (_selectedIndex == nextIndex)
            {
                return;
            }

            _selectedIndex = nextIndex;
            RaiseAllPropertiesChanged();
        }
    }

    public int Count => _items.Count;

    public string CurrentIndexText => string.Empty;

    public object? CurrentHeader => null;

    public object CurrentContent => _items.Count == 0
        ? string.Empty
        : YamlDocumentationViewFactory.Create(_items[SelectedIndex]);

    private void RaiseAllPropertiesChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedIndex)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentIndexText)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentHeader)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentContent)));
    }
}

internal static class YamlDocumentationViewFactory
{
    public static Control Create(YamlDocumentationInfo documentation)
    {
        var panel = new StackPanel
        {
            Spacing = 6,
            MaxWidth = 520,
        };

        panel.Children.Add(new TextBlock
        {
            Text = documentation.Label,
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap,
        });

        panel.Children.Add(new TextBlock
        {
            Text = FormatTypeDisplayName(documentation.Type),
            FontSize = 12,
            Opacity = 0.78,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Left,
        });

        if (!string.IsNullOrWhiteSpace(documentation.PropertySummary))
        {
            panel.Children.Add(CreateSummaryBlock(documentation.PropertySummary));
        }
        else if (!string.IsNullOrWhiteSpace(documentation.TypeSummary))
        {
            panel.Children.Add(CreateSummaryBlock(documentation.TypeSummary));
        }

        return panel;
    }

    private static Control CreateSummaryBlock(string content)
    {
        return new TextBlock
        {
            Text = content,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Left,
        };
    }

    private static string FormatTypeDisplayName(Type type)
    {
        var normalizedType = Nullable.GetUnderlyingType(type) ?? type;
        if (normalizedType.IsArray)
        {
            return $"{FormatTypeDisplayName(normalizedType.GetElementType() ?? typeof(object))}[]";
        }

        if (!normalizedType.IsGenericType)
        {
            return normalizedType.FullName ?? normalizedType.Name;
        }

        var genericTypeDefinition = normalizedType.GetGenericTypeDefinition();
        var genericTypeName = genericTypeDefinition.FullName ?? genericTypeDefinition.Name;
        var tickIndex = genericTypeName.IndexOf('`');
        if (tickIndex >= 0)
        {
            genericTypeName = genericTypeName[..tickIndex];
        }

        var genericArguments = normalizedType.GetGenericArguments()
            .Select(FormatTypeDisplayName);

        return $"{genericTypeName}<{string.Join(", ", genericArguments)}>";
    }
}
