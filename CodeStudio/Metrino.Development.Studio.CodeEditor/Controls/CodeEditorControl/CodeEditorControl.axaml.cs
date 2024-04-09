using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Controls.Primitives;

using AvaloniaEdit;
using AvaloniaEdit.Document;
//using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Folding;
using AvaloniaEdit.TextMate;

using TextMateSharp.Grammars;

using System;
using System.Windows.Input;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Metrino.Development.Studio.Library.Controls;

using KeyGesture = Avalonia.Input.KeyGesture;
using Key = Avalonia.Input.Key;

public partial class CodeEditorControl : TemplatedControl
{
    private TextEditor _textEditor;
    private FoldingManager _foldingManager;
    private TextMate.Installation _textMateInstallation;
    private CompletionListPopup _completionWindow;

    //private OverloadInsightWindow _insightWindow;
    //private Button _addControlButton;
    //private Button _clearControlButton;
    //private Button _changeThemeButton;
    //private ComboBox _syntaxModeCombo;

    private TextBlock _statusTextBlock;
    private LineElementGenerator _generator = new LineElementGenerator();
    private RegistryOptions _registryOptions;
    private int _currentTheme = (int)ThemeName.DarkPlus;


    public static readonly StyledProperty<string> LanguageProperty
        = AvaloniaProperty.Register<CodeEditorControl, string>(nameof(Language));

    public string Language
    {
        get => GetValue(LanguageProperty);
        set => SetValue(LanguageProperty, value);
    }

    public static readonly StyledProperty<TextDocument> DocumentProperty
        = AvaloniaProperty.Register<CodeEditorControl, TextDocument>(nameof(Document));

    public TextDocument Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<ICompletionData>> CompletionsProperty
        = AvaloniaProperty.Register<CodeEditorControl, ObservableCollection<ICompletionData>>(nameof(Completions));

    public ObservableCollection<ICompletionData> Completions
    {
        get => GetValue(CompletionsProperty);
        set => SetValue(CompletionsProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<string>> InsightsProperty
        = AvaloniaProperty.Register<CodeEditorControl, ObservableCollection<string>>(nameof(Insights));

    public ObservableCollection<string> Insights
    {
        get => GetValue(InsightsProperty);
        set => SetValue(InsightsProperty, value);
    }

    public static readonly StyledProperty<ICommand> SaveCommandProperty
        = AvaloniaProperty.Register<CodeEditorControl, ICommand>(nameof(SaveCommand));

    /// <summary>
    /// Gets or sets the icon to display.
    /// </summary>
    public ICommand SaveCommand
    {
        get => GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> ExecuteCommandProperty
        = AvaloniaProperty.Register<CodeEditorControl, ICommand>(nameof(ExecuteCommand));

    /// <summary>
    /// Gets or sets the icon to display.
    /// </summary>
    public ICommand ExecuteCommand
    {
        get => GetValue(ExecuteCommandProperty);
        set => SetValue(ExecuteCommandProperty, value);
    }


    public static readonly StyledProperty<bool> CommandModeProperty
        = AvaloniaProperty.Register<CodeEditorControl, bool>(nameof(CommandMode));

    public bool CommandMode
    {
        get => GetValue(CommandModeProperty);
        set => SetValue(CommandModeProperty, value);
    }

    static CodeEditorControl()
    {
        DocumentProperty.Changed.AddClassHandler<CodeEditorControl>(OnDocumentChanged);
        CompletionsProperty.Changed.AddClassHandler<CodeEditorControl>(OnCompletionsPropertyChanged);
        InsightsProperty.Changed.AddClassHandler<CodeEditorControl>(OnInsightsPropertyChanged);
        CommandModeProperty.Changed.AddClassHandler<CodeEditorControl>(OnCommandModeChanged);
    }

    private static void OnInsightsPropertyChanged(CodeEditorControl control, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue != null)
        {
            if (args.OldValue is ObservableCollection<string>)
                (args.OldValue as ObservableCollection<string>).CollectionChanged -= control.OnInsightsListChanged;
        }

        if (args.NewValue == null)
        {
            //if (control._insightWindow != null)
            //    control._insightWindow.Close();
        }
        else
        {
            (args.NewValue as ObservableCollection<string>).CollectionChanged += control.OnInsightsListChanged;
        }
    }

    private static void OnCommandModeChanged(CodeEditorControl control, AvaloniaPropertyChangedEventArgs args)
    {
        if (control._textEditor == null)
            return;

        if (args.NewValue == null)
        {
        }
        else
        {
            if((bool)args.NewValue)
            {
                control._textEditor.TextArea.Caret.Hide();
            }
            else
            {
                control._textEditor.TextArea.Caret.Show();
            }
        }
    }

    private void OnInsightsListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {

    }

    private static void OnDocumentChanged(CodeEditorControl control, AvaloniaPropertyChangedEventArgs args)
    {
        if (control._textEditor == null) return;

        if (control.Document != null)
        {
            control._textEditor.Document = control.Document;
            if (control.Document.FileName != null)
            {
                var languageID = Path.GetExtension(control.Document.FileName);
                var language = control._registryOptions.GetLanguageByExtension(languageID);

                control._textMateInstallation.SetGrammar(control._registryOptions.GetScopeByLanguageId(language.Id));
            }
        }
    }

    private static void OnCompletionsPropertyChanged(CodeEditorControl control, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue != null)
        {
            if (args.OldValue is ObservableCollection<ICompletionData>)
                (args.OldValue as ObservableCollection<ICompletionData>).CollectionChanged -= control.OnCompletionListChanged;
        }

        if (args.NewValue == null)
        {
            if (control._completionWindow != null)
                control._completionWindow.Close();
        }
        else
        {
            (args.NewValue as ObservableCollection<ICompletionData>).CollectionChanged += control.OnCompletionListChanged;
        }
    }

    private void OnCompletionListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (Completions == null && _completionWindow != null && _completionWindow.IsOpen)
        {
            _completionWindow.Close();
            return;
        }

        if (Completions != null && Completions.Count == 0 && _completionWindow != null && _completionWindow.IsOpen)
        {
            _completionWindow.Close();
            return;
        }

        if (_completionWindow == null)
        {
            _completionWindow = new CompletionListPopup(_textEditor.TextArea)
            {
                CloseAutomatically = false
            };
            _completionWindow.Closed += (o, args) => { _completionWindow = null; };

            var data = _completionWindow.CompletionList.CompletionData;
            foreach (var item in Completions)
                data.Add(item);

            _completionWindow.Show();
        }
        else
        {
            _completionWindow.CompletionList.CompletionData.Clear();
            var data = _completionWindow.CompletionList.CompletionData;
            foreach (var item in Completions)
                data.Add(item);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _textEditor = e.NameScope.Find<TextEditor>("Editor");
        if (_textEditor == null)
            return;

        _textEditor.Background = Brushes.Transparent;
        _textEditor.ShowLineNumbers = true;
        //_textEditor.ContextMenu = new ContextMenu
        //{
        //    ItemsSource = new List<MenuItem>
        //    {
        //        new MenuItem
        //        {
        //            Header = "Run",
        //            InputGesture = new KeyGesture(Key.F5),
        //            Command=ExecuteCommand
        //        },
        //        new MenuItem
        //        {
        //            Header = "-"
        //        },
        //        new MenuItem
        //        {
        //            Header = "Save",
        //            InputGesture = new KeyGesture(Key.S, KeyModifiers.Control),
        //            Command=SaveCommand
        //        },
        //        new MenuItem
        //        {
        //            Header = "-"
        //        },
        //        new MenuItem
        //        {
        //            Header = "Cut",
        //            InputGesture = new KeyGesture(Key.X, KeyModifiers.Control)
        //        },
        //        new MenuItem
        //        {
        //            Header = "Copy",
        //            InputGesture = new KeyGesture(Key.C, KeyModifiers.Control)
        //        },
        //        new MenuItem
        //        {
        //            Header = "-"
        //        },
        //        new MenuItem
        //        {
        //            Header = "Paste",
        //            InputGesture = new KeyGesture(Key.V, KeyModifiers.Control)
        //        }
        //    }
        //};

        _textEditor.TextArea.Background = Background;
        _textEditor.Options = new TextEditorOptions
        {
            ShowBoxForControlCharacters = true,
            ColumnRulerPositions = new List<int>() { 125 },
            ShowColumnRulers = true,
            ConvertTabsToSpaces = true,
            WordWrapIndentation = 15.0,
            EnableRectangularSelection = true,
        };

        var numberColumn = _textEditor.TextArea.LeftMargins.FirstOrDefault();
        if (numberColumn != null)
        {
            numberColumn.Width = 40;
            numberColumn.Margin = new Thickness(10, 0, 10, 0);
        }

        _textEditor.TextArea.TextView.Margin = new Thickness(15, 0, 0, 0);
        _textEditor.TextArea.RightClickMovesCaret = true;
        _textEditor.TextArea.SelectionCornerRadius = 0;
        _textEditor.TextArea.SelectionBrush = new SolidColorBrush(Color.FromArgb(255, 38, 79, 120));
        _textEditor.TextArea.TextView.LineTransformers.Add(new TextDecorationTransformer());
        _textEditor.TextArea.TextView.ElementGenerators.Add(_generator);

        _registryOptions = new RegistryOptions((ThemeName)_currentTheme);

        _textMateInstallation = _textEditor.InstallTextMate(_registryOptions);

        if (Document != null)
        {
            _textEditor.Document = Document;
            if (Document.FileName != null)
            {
                var languageID = Path.GetExtension(Document.FileName);
                var language = _registryOptions.GetLanguageByExtension(languageID);

                _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(language.Id));
            }
        }

        ConfigureEventHandlers();
    }

    private void ConfigureEventHandlers()
    {
        _textEditor.TextArea.KeyDown += (s, e) =>
        {
            if (e.PhysicalKey == PhysicalKey.Escape)
                TopLevel.GetTopLevel(_textEditor.TextArea).Focus();
                
        };
        _textEditor.TextArea.TextEntered += Editor_TextArea_TextEntered;
        _textEditor.TextArea.TextEntering += Editor_TextArea_TextEntering;
        _textEditor.TextArea.Caret.PositionChanged += Editor_TextArea_Caret_PositionChanged;
    }

    private void Editor_TextArea_Caret_PositionChanged(object? sender, EventArgs e)
    {
    }

    private void Editor_TextArea_TextEntering(object? sender, TextInputEventArgs e)
    {
    }

    private void Editor_TextArea_TextEntered(object? sender, TextInputEventArgs e)
    {
    }
}