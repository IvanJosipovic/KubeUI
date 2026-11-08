using System.Collections;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Folding;
using KubeUI.Features.Resources.Yaml.Behaviors;
using Shouldly;

namespace KubeUI.Features.Resources.Tests.Yaml;

public sealed class YamlFoldingBehaviorTests
{
    [AvaloniaFact]
    public void Detaching_folding_behavior_releases_retained_marker_children()
    {
        var editor = new TextEditor();
        var behavior = new YamlFoldingBehavior();
        var behaviors = Interaction.GetBehaviors(editor);
        behaviors.Add(behavior);

        var margin = editor.TextArea.LeftMargins.OfType<FoldingMargin>().Single();
        var logicalChildren = ((ILogical)margin).LogicalChildren;
        var mutableChildren = (IList)logicalChildren;
        for (var index = 0; index < 100; index++)
        {
            mutableChildren.Add(new Button());
        }

        mutableChildren.Count.ShouldBe(100);

        behaviors.Remove(behavior);

        mutableChildren.Count.ShouldBe(0);
    }
}
