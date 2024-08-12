﻿using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;
using AvaloniaEdit;
using static AvaloniaEdit.TextMate.TextMate;
using TextMateSharp.Grammars;

namespace KubeUI.Views;

public sealed class PodConsoleView : MyViewBase<PodConsoleViewModel>
{
    private Installation _textMateInstallation;

    private RegistryOptions _registryOptions;

    public PodConsoleView()
    {
        _registryOptions = new RegistryOptions(Application.Current.ActualThemeVariant == ThemeVariant.Light ? ThemeName.Light : ThemeName.DarkPlus);

        Application.Current.ActualThemeVariantChanged += Current_ActualThemeVariantChanged;
    }

    private void Current_ActualThemeVariantChanged(object? sender, EventArgs e)
    {
        if (Application.Current.ActualThemeVariant == ThemeVariant.Light)
        {
            _textMateInstallation.SetTheme(_registryOptions.LoadTheme(ThemeName.Light));
        }
        else if (Application.Current.ActualThemeVariant == ThemeVariant.Dark)
        {
            _textMateInstallation.SetTheme(_registryOptions.LoadTheme(ThemeName.DarkPlus));
        }
    }

    protected override object Build(PodConsoleViewModel? vm)
    {
        return new Grid()
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .VerticalAlignment(VerticalAlignment.Stretch)
            .Rows("Auto,*")
            .Children([
                new StackPanel()
                    .Row(0)
                    .Orientation(Orientation.Horizontal)
                    .Children([
                        new Label()
                            .Margin(0,0,4,0)
                            .Content("Pod: "),
                        new Label()
                            .Margin(0,0,4,0)
                            .Content(new MultiBinding(){
                                Bindings = [
                                        new Binding("Object.Metadata.NamespaceProperty"),
                                        new Binding("Object.Metadata.Name"),
                                        new Binding(nameof(PodConsoleViewModel.ContainerName))
                                ],
                                StringFormat = "{0}/{1}/{2}"
                            })
                    ]),
                new TextEditor()
                    .Ref(out var editor)
                    .Row(1)
                    .Set(x => {
                        _textMateInstallation = editor.InstallTextMate(_registryOptions, false);

                        x.Options.AllowScrollBelowDocument = false;
                        x.Options.ShowBoxForControlCharacters = false;
                        x.Options.EnableHyperlinks = false;
                        x.Options.EnableEmailHyperlinks = false;
                        x.TextChanged += (sender, e) => {
                            editor.ScrollToEnd();
                        };
                        return x;
                    })
                    .Set(TextEditor.DocumentProperty, @vm.Console, BindingMode.OneWay)
                    .Set(TextEditor.FontFamilyProperty, new FontFamily("Consolas,Menlo,Monospace"))
                    .Set(TextEditor.FontSizeProperty, 14.0)
                    .Set(TextEditor.FontWeightProperty, FontWeight.Normal)
                    .Set(TextEditor.IsReadOnlyProperty, true)
                    .Set(TextEditor.ShowLineNumbersProperty, false)
                    .HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
                    .VerticalScrollBarVisibility(ScrollBarVisibility.Visible)
                    .OnKeyUp((e) => {
                        if (DataContext is PodConsoleViewModel dc1)
                        {
                            if (e.KeyModifiers is KeyModifiers.Control)
                            {
                                if (e.Key == Key.C)
                                {
                                    editor.Copy();
                                    return;
                                }
                                if (e.Key == Key.V)
                                {
                                    _ = dc1.Paste();
                                    return;
                                }
                            }

                            dc1.Send(e.KeySymbol);
                        }
                    })
                    .ContextMenu([
                            new MenuItem()
                                .OnClick((_) => editor.Copy())
                                .Header(Assets.Resources.Action_Copy)
                                .InputGesture(new KeyGesture(Key.C, KeyModifiers.Control))
                                .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("copy_regular") }),
                            new MenuItem()
                                .Command(vm.PasteCommand)
                                .Header(Assets.Resources.Action_Paste)
                                .InputGesture(new KeyGesture(Key.V, KeyModifiers.Control))
                                .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("clipboard_paste_regular") }),
                        ]),
            ]);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        Application.Current.ActualThemeVariantChanged -= Current_ActualThemeVariantChanged;
    }
}