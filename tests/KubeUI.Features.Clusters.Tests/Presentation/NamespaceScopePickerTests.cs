using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using k8s.Models;
using KubeUI.Features.Clusters.Presentation;
using Shouldly;
using Ursa.Controls;

namespace KubeUI.Features.Clusters.Tests.Presentation;

public sealed class NamespaceScopePickerTests
{
    [AvaloniaFact]
    public void Defaults_show_localized_labels_and_configurable_width()
    {
        var control = new NamespaceScopePicker();
        using var window = Application.Current.CreateTestWindow(content: control);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var picker = Picker(control);
        var link = Link(control);
        control.Namespaces.ShouldBeNull();
        control.SelectedNamespaces.ShouldBeNull();
        control.IsSelectionLinked.ShouldBeTrue();
        control.IsSelectionEnabled.ShouldBeTrue();
        control.IsLinkEnabled.ShouldBeTrue();
        control.PickerWidth.ShouldBe(200);
        picker.Width.ShouldBe(200);
        picker.IsEnabled.ShouldBeTrue();
        link.IsEnabled.ShouldBeTrue();
        link.IsChecked.ShouldBe(true);
        picker.PlaceholderText.ShouldBe(ClustersLanguageManager.Instance.NamespaceScopePicker_SelectNamespace.CurrentValue());
        ToolTip.GetTip(link).ShouldBe(ClustersLanguageManager.Instance.NamespaceScopePicker_LinkNamespaces.CurrentValue());

        control.PickerWidth = 280;
        Dispatcher.UIThread.RunJobs();
        picker.Width.ShouldBe(280);
    }

    [AvaloniaFact]
    public void Source_and_selection_keep_caller_references_and_retarget_on_replacement()
    {
        var first = Namespace("team-a");
        var second = Namespace("team-b");
        var source = new ObservableCollection<V1Namespace> { first, second };
        var selected = new ObservableCollection<V1Namespace> { first };
        var control = new NamespaceScopePicker { Namespaces = source, SelectedNamespaces = selected };
        using var window = Application.Current.CreateTestWindow(content: control);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var picker = Picker(control);
        control.Namespaces.ShouldBeSameAs(source);
        picker.ItemsSource.ShouldBeSameAs(source);
        control.SelectedNamespaces.ShouldBeSameAs(selected);
        picker.SelectedItems.ShouldBeSameAs(selected);

        // Keep both selected values valid while changing sources: Ursa owns item removal policy.
        var replacementSource = new ObservableCollection<V1Namespace> { first, second, Namespace("team-c") };
        var replacementSelection = new ObservableCollection<V1Namespace> { second };
        control.Namespaces = replacementSource;
        control.SelectedNamespaces = replacementSelection;
        Dispatcher.UIThread.RunJobs();

        control.Namespaces.ShouldBeSameAs(replacementSource);
        picker.ItemsSource.ShouldBeSameAs(replacementSource);
        control.SelectedNamespaces.ShouldBeSameAs(replacementSelection);
        picker.SelectedItems.ShouldBeSameAs(replacementSelection);
        picker.SelectedItems!.Add(first);
        replacementSelection.ShouldBe(new[] { second, first });
        selected.ShouldBe(new[] { first });
        selected.Add(second);
        replacementSelection.ShouldBe(new[] { second, first });
    }

    [AvaloniaFact]
    public void Selection_mutations_update_same_caller_list_and_empty_has_no_implicit_policy()
    {
        var first = Namespace("team-a");
        var second = Namespace("team-b");
        var source = new ObservableCollection<V1Namespace> { first, second };
        var selected = new ObservableCollection<V1Namespace>();
        var control = new NamespaceScopePicker { Namespaces = source, SelectedNamespaces = selected };
        using var window = Application.Current.CreateTestWindow(content: control);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var picker = Picker(control);
        picker.SelectedItems.ShouldBeSameAs(selected);
        selected.ShouldBeEmpty();
        picker.SelectedItems!.Add(first);
        selected.ShouldBe(new[] { first });
        picker.SelectedItems.Add(second);
        selected.ShouldBe(new[] { first, second });
        picker.SelectedItems.Remove(first);
        selected.ShouldBe(new[] { second });
        picker.SelectedItems.Clear();
        Dispatcher.UIThread.RunJobs();

        selected.ShouldBeEmpty();
        control.SelectedNamespaces.ShouldBeSameAs(selected);
        picker.SelectedItems.ShouldBeSameAs(selected);
        source.ShouldBe(new[] { first, second });
        control.IsSelectionLinked.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Link_toggle_is_two_way_without_replacing_selection_or_implementing_link_policy()
    {
        var item = Namespace("team-a");
        var source = new[] { item };
        var selected = new ObservableCollection<V1Namespace> { item };
        var control = new NamespaceScopePicker { Namespaces = source, SelectedNamespaces = selected };
        using var window = Application.Current.CreateTestWindow(content: control);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var link = Link(control);
        link.SetCurrentValue(ToggleButton.IsCheckedProperty, false);
        Dispatcher.UIThread.RunJobs();
        control.IsSelectionLinked.ShouldBeFalse();

        control.IsSelectionLinked = true;
        Dispatcher.UIThread.RunJobs();
        link.IsChecked.ShouldBe(true);
        link.SetCurrentValue(ToggleButton.IsCheckedProperty, false);
        Dispatcher.UIThread.RunJobs();
        control.IsSelectionLinked.ShouldBeFalse();
        link.SetCurrentValue(ToggleButton.IsCheckedProperty, true);
        Dispatcher.UIThread.RunJobs();
        control.IsSelectionLinked.ShouldBeTrue();
        control.Namespaces.ShouldBeSameAs(source);
        control.SelectedNamespaces.ShouldBeSameAs(selected);
        Picker(control).SelectedItems.ShouldBeSameAs(selected);
        selected.ShouldBe(new[] { item });
    }

    [AvaloniaFact]
    public void Selection_and_link_enabled_flags_are_independent_and_allow_read_only_scope()
    {
        var item = Namespace("team-a");
        var selected = new ObservableCollection<V1Namespace> { item };
        var control = new NamespaceScopePicker { Namespaces = new[] { item }, SelectedNamespaces = selected };
        using var window = Application.Current.CreateTestWindow(content: control);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var picker = Picker(control);
        var link = Link(control);
        control.IsSelectionEnabled = false;
        Dispatcher.UIThread.RunJobs();
        picker.IsEnabled.ShouldBeFalse();
        link.IsEnabled.ShouldBeTrue();

        control.IsLinkEnabled = false;
        Dispatcher.UIThread.RunJobs();
        picker.IsEnabled.ShouldBeFalse();
        link.IsEnabled.ShouldBeFalse();
        selected.ShouldBe(new[] { item });
        control.IsSelectionLinked.ShouldBeTrue();

        control.IsSelectionEnabled = true;
        Dispatcher.UIThread.RunJobs();
        picker.IsEnabled.ShouldBeTrue();
        link.IsEnabled.ShouldBeFalse();
        control.IsLinkEnabled = true;
        Dispatcher.UIThread.RunJobs();
        link.IsEnabled.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Independent_instances_do_not_share_source_selection_link_or_enabled_state()
    {
        var firstItem = Namespace("team-a");
        var secondItem = Namespace("team-b");
        var firstSelection = new ObservableCollection<V1Namespace> { firstItem };
        var secondSelection = new ObservableCollection<V1Namespace> { secondItem };
        var first = new NamespaceScopePicker { Namespaces = new[] { firstItem }, SelectedNamespaces = firstSelection };
        var second = new NamespaceScopePicker { Namespaces = new[] { secondItem }, SelectedNamespaces = secondSelection };
        var host = new StackPanel { Children = { first, second } };
        using var window = Application.Current.CreateTestWindow(content: host);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        Picker(first).SelectedItems!.Clear();
        Link(first).SetCurrentValue(ToggleButton.IsCheckedProperty, false);
        first.IsSelectionEnabled = false;
        first.IsLinkEnabled = false;
        first.PickerWidth = 300;
        Dispatcher.UIThread.RunJobs();

        firstSelection.ShouldBeEmpty();
        first.IsSelectionLinked.ShouldBeFalse();
        secondSelection.ShouldBe(new[] { secondItem });
        Picker(second).ItemsSource.ShouldBeSameAs(second.Namespaces);
        Picker(second).SelectedItems.ShouldBeSameAs(secondSelection);
        second.Namespaces.ShouldNotBeSameAs(first.Namespaces);
        second.IsSelectionLinked.ShouldBeTrue();
        Link(second).IsChecked.ShouldBe(true);
        Picker(second).IsEnabled.ShouldBeTrue();
        Link(second).IsEnabled.ShouldBeTrue();
        Picker(second).Width.ShouldBe(200);
    }

    [AvaloniaFact]
    public void Attach_detach_and_reattach_preserve_caller_source_selection_and_link_state()
    {
        var item = Namespace("team-a");
        var source = new ObservableCollection<V1Namespace> { item };
        var selected = new ObservableCollection<V1Namespace> { item };
        var control = new NamespaceScopePicker
        {
            Namespaces = source,
            SelectedNamespaces = selected,
            IsSelectionLinked = false
        };
        using var window = Application.Current.CreateTestWindow(content: control);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        var picker = Picker(control);
        var link = Link(control);
        AssertScope();

        window.Content = null;
        Dispatcher.UIThread.RunJobs();
        AssertScope();
        window.Content = control;
        Dispatcher.UIThread.RunJobs();
        AssertScope();

        void AssertScope()
        {
            control.Namespaces.ShouldBeSameAs(source);
            picker.ItemsSource.ShouldBeSameAs(source);
            control.SelectedNamespaces.ShouldBeSameAs(selected);
            picker.SelectedItems.ShouldBeSameAs(selected);
            selected.ShouldBe(new[] { item });
            control.IsSelectionLinked.ShouldBeFalse();
            link.IsChecked.ShouldBe(false);
        }
    }

    private static MultiComboBox Picker(NamespaceScopePicker control)
        => control.GetVisualDescendants().OfType<MultiComboBox>().Single();

    private static ToggleButton Link(NamespaceScopePicker control)
        => control.GetVisualDescendants().OfType<ToggleButton>()
            .Single(toggle => Equals(ToolTip.GetTip(toggle), ClustersLanguageManager.Instance.NamespaceScopePicker_LinkNamespaces.CurrentValue()));

    private static V1Namespace Namespace(string name)
        => new() { Metadata = new V1ObjectMeta { Name = name } };
}
