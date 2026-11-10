using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using KubeUI.Controls.Styles;
using Shouldly;
using Ursa.Controls;

namespace KubeUI.Controls.Tests;

public sealed class SharedControlStylesHostTests
{
    [AvaloniaFact]
    public void Ursa_picker_styles_and_behavior_work_without_app_or_feature_references()
    {
        Application.Current.ShouldBeOfType<Infra.MinimalControlsTestApplication>();
        Application.Current.Styles.OfType<SharedControlStyles>().Count().ShouldBe(1);

        var selectedItems = new AvaloniaList<object> { "selected" };
        var picker = new MultiComboBox
        {
            ItemsSource = new[] { "selected", "available" },
            SelectedItems = selectedItems
        };
        var editor = new TextEditor();
        var content = new StackPanel { Children = { picker, editor } };
        var window = new Window { Content = content, Width = 420, Height = 280 };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var selectedItemList = picker.GetVisualDescendants().OfType<MultiComboBoxSelectedItemList>().Single();
            Interaction.GetBehaviors(selectedItemList).ShouldNotBeNull().ShouldNotBeEmpty();
            editor.Background.ShouldBeOfType<ImmutableSolidColorBrush>().Color.ShouldNotBe(Colors.Transparent);
            var lightEditorColor = ((ImmutableSolidColorBrush)editor.Background!).Color;

            Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
            Dispatcher.UIThread.RunJobs();
            selectedItemList.IsEffectivelyVisible.ShouldBeTrue();
            var darkEditorColor = editor.Background.ShouldBeOfType<ImmutableSolidColorBrush>().Color;
            darkEditorColor.ShouldNotBe(lightEditorColor);

            Application.Current.RequestedThemeVariant = ThemeVariant.Light;
            Dispatcher.UIThread.RunJobs();
            selectedItemList.IsEffectivelyVisible.ShouldBeTrue();
            editor.Background.ShouldBeOfType<ImmutableSolidColorBrush>().Color.ShouldBe(lightEditorColor);
        }
        finally
        {
            window.Close();
        }
    }
}
