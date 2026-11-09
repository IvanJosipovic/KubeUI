using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KubeUI.Controls.Presentation;
using Shouldly;

namespace KubeUI.App.Tests.Controls.Presentation;

public sealed class ExpandableSectionTests
{
    [AvaloniaFact]
    public void Constructor_preserves_expanded_defaults_and_inheritable_expander_style()
    {
        var section = new DerivedExpandableSection();

        section.IsExpanded.ShouldBeTrue();
        section.HorizontalAlignment.ShouldBe(HorizontalAlignment.Stretch);
        section.VerticalAlignment.ShouldBe(VerticalAlignment.Top);
        section.Header.ShouldBeNull();
        section.Content.ShouldBeNull();
        section.StyleKey.ShouldBe(typeof(Expander));
    }

    [AvaloniaFact]
    public void Compact_resources_are_consumed_by_the_expander_theme()
    {
        var section = new ExpandableSection { Header = "Properties" };
        var window = new Window { Content = section };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            section.Resources["ExpanderMinHeight"].ShouldBe(28d);
            section.Resources["ExpanderHeaderPadding"].ShouldBe(new Thickness(10, 0, 0, 0));
            section.Resources["ExpanderChevronMargin"].ShouldBe(new Thickness(8, 0, 4, 0));
            section.Resources["ExpanderChevronButtonSize"].ShouldBe(24d);
            section.Resources["ExpanderContentPadding"].ShouldBe(new Thickness(5, 0, 0, 0));

            section.MinHeight.ShouldBe(28d);
            section.Padding.ShouldBe(new Thickness(5, 0, 0, 0));
            HeaderToggle(section).Padding.ShouldBe(new Thickness(10, 0, 0, 0));
            var chevron = section.GetVisualDescendants().OfType<Border>()
                .Single(border => border.Name == "ExpandCollapseChevronBorder");
            chevron.Width.ShouldBe(24d);
            chevron.Height.ShouldBe(24d);
            chevron.Margin.ShouldBe(new Thickness(8, 0, 4, 0));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Local_resource_overrides_update_rendered_spacing_without_changing_siblings()
    {
        var customized = new ExpandableSection();
        var unchanged = new ExpandableSection();
        var panel = new StackPanel { Children = { customized, unchanged } };
        var window = new Window { Content = panel };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            customized.Resources["ExpanderMinHeight"] = 40d;
            customized.Resources["ExpanderHeaderPadding"] = new Thickness(12, 2, 3, 4);
            customized.Resources["ExpanderContentPadding"] = new Thickness(9);
            customized.Resources["ExpanderChevronButtonSize"] = 30d;
            customized.Resources["ExpanderChevronMargin"] = new Thickness(2);
            Dispatcher.UIThread.RunJobs();

            customized.MinHeight.ShouldBe(40d);
            customized.Padding.ShouldBe(new Thickness(9));
            HeaderToggle(customized).Padding.ShouldBe(new Thickness(12, 2, 3, 4));
            var chevron = customized.GetVisualDescendants().OfType<Border>()
                .Single(border => border.Name == "ExpandCollapseChevronBorder");
            chevron.Width.ShouldBe(30d);
            chevron.Height.ShouldBe(30d);
            chevron.Margin.ShouldBe(new Thickness(2));
            unchanged.MinHeight.ShouldBe(28d);
            unchanged.Padding.ShouldBe(new Thickness(5, 0, 0, 0));
            HeaderToggle(unchanged).Padding.ShouldBe(new Thickness(10, 0, 0, 0));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Header_and_content_controls_are_presented_and_can_be_replaced()
    {
        var header = new TextBlock { Text = "Header" };
        var content = new TextBlock { Text = "Content" };
        var section = new ExpandableSection { Header = header, Content = content };
        var window = new Window { Content = section };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            section.GetVisualDescendants().ShouldContain(header);
            section.GetVisualDescendants().ShouldContain(content);
            HeaderToggle(section).Content.ShouldBeSameAs(header);

            var replacementHeader = new TextBlock { Text = "Updated header" };
            var replacementContent = new TextBlock { Text = "Updated content" };
            section.Header = replacementHeader;
            section.Content = replacementContent;
            Dispatcher.UIThread.RunJobs();

            HeaderToggle(section).Content.ShouldBeSameAs(replacementHeader);
            section.GetVisualDescendants().ShouldContain(replacementHeader);
            section.GetVisualDescendants().ShouldContain(replacementContent);
            section.GetVisualDescendants().ShouldNotContain(header);
            section.GetVisualDescendants().ShouldNotContain(content);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Expansion_binding_and_content_visibility_survive_theme_changes_and_reattachment()
    {
        var content = new TextBlock { Text = "Section content" };
        var section = new DerivedExpandableSection { Header = "Section", Content = content };
        var window = new Window { Content = section };

        try
        {
            window.Show();

            foreach (var theme in new[] { ThemeVariant.Light, ThemeVariant.Dark })
            {
                window.RequestedThemeVariant = theme;
                Dispatcher.UIThread.RunJobs();

                HeaderToggle(section).IsChecked.ShouldBe(true);
                content.IsEffectivelyVisible.ShouldBeTrue();

                // Preserve the template binding, as ToggleButton does when toggled by input.
                HeaderToggle(section).SetCurrentValue(ToggleButton.IsCheckedProperty, false);
                Dispatcher.UIThread.RunJobs();
                section.IsExpanded.ShouldBeFalse();
                content.IsEffectivelyVisible.ShouldBeFalse();

                window.Content = null;
                Dispatcher.UIThread.RunJobs();
                TopLevel.GetTopLevel(content).ShouldBeNull();
                window.Content = section;
                Dispatcher.UIThread.RunJobs();
                section.IsExpanded.ShouldBeFalse();
                HeaderToggle(section).IsChecked.ShouldBe(false);
                content.IsEffectivelyVisible.ShouldBeFalse();

                section.IsExpanded = true;
                Dispatcher.UIThread.RunJobs();
                HeaderToggle(section).IsChecked.ShouldBe(true);
                content.IsEffectivelyVisible.ShouldBeTrue();

                HeaderToggle(section).SetCurrentValue(ToggleButton.IsCheckedProperty, false);
                section.IsExpanded.ShouldBeFalse();
                HeaderToggle(section).SetCurrentValue(ToggleButton.IsCheckedProperty, true);
                section.IsExpanded.ShouldBeTrue();
            }
        }
        finally
        {
            window.Close();
        }
    }

    private static ToggleButton HeaderToggle(ExpandableSection section) =>
        section.GetVisualDescendants().OfType<ToggleButton>()
            .Single(toggle => toggle.Name == "ExpanderHeader");

    private sealed class DerivedExpandableSection : ExpandableSection
    {
    }
}
