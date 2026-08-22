using Avalonia.Controls.Documents;
using Avalonia.Markup.Xaml.MarkupExtensions;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using KubeUI.Avalonia.Styles;

namespace KubeUI.Avalonia.Features.Resources.Yaml;

internal sealed class YamlCompletionData(YamlCompletionItemInfo item) : ICompletionData
{
    public string Text => item.Text;

    public object Content => item.Text;

    public object Description => item.Documentation == null
        ? item.Schema.TypeName
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
        if (!item.Schema.IsObject && !item.Schema.IsSequence)
        {
            return item.InsertionText;
        }

        var line = textArea.Document.GetLineByOffset(completionSegment.Offset);
        var keyIndent = completionSegment.Offset - line.Offset;
        var childIndent = keyIndent + 2;
        var newLine = textArea.Document.Text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        if (item.Schema.IsSequence)
        {
            return $"{item.Text}:{newLine}{new string(' ', childIndent)}- ";
        }

        return $"{item.Text}:{newLine}{new string(' ', childIndent)}";
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

        var typeText = new TextBlock
        {
            Text = documentation.TypeName,
            Opacity = 0.78,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Left,
        };
        typeText
            .FontFamily(new DynamicResourceExtension(Typography.CodeFontFamilyResourceKey))
            .FontSize(new DynamicResourceExtension(Typography.CodeFontSizeResourceKey));
        panel.Children.Add(typeText);

        if (!string.IsNullOrWhiteSpace(documentation.PropertySummary))
        {
            panel.Children.Add(CreateSummaryBlock(documentation.PropertySummary));
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

}
